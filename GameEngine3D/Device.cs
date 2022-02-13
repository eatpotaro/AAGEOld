using Windows.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Collections.Generic;
using SharpDX;
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SoftEngine;
using System.Linq;

namespace GameEngine3D
{
    public class Device
    {
        private byte[] backBuffer;
        private readonly float[] depthBuffer;
        private object[] lockBuffer;
        private WriteableBitmap bmp;
        private readonly int renderWidth;
        private readonly int renderHeight;

        public List<Light> Lights = new List<Light>();
        public Device(WriteableBitmap bmp)
        {
            this.bmp = bmp;
            renderWidth = bmp.PixelWidth;
            renderHeight = bmp.PixelHeight;

            // the back buffer size is equal to the number of pixels to draw
            // on screen (width*height) * 4 (R,G,B & Alpha values). 
            backBuffer = new byte[renderWidth * renderHeight * 4];
            depthBuffer = new float[renderWidth * renderHeight];
            lockBuffer = new object[renderWidth * renderHeight];
            for (var i = 0; i < lockBuffer.Length; i++)
            {
                lockBuffer[i] = new object();
            }
        }

        // Called to put a pixel on screen at a specific X,Y coordinates
        public void PutPixel(int x, int y, float z, Color4 color)
        {
            // As we have a 1-D Array for our back buffer
            // we need to know the equivalent cell in 1-D based
            // on the 2D coordinates on screen
            var index = (x + y * renderWidth);
            var index4 = index * 4;

            // Protecting our buffer against threads concurrencies
            lock (lockBuffer[index])
            {
                if (depthBuffer[index] < z)
                {
                    return; // Discard
                }

                depthBuffer[index] = z;

                backBuffer[index4] = (byte)(color.Blue * 255);
                backBuffer[index4 + 1] = (byte)(color.Green * 255);
                backBuffer[index4 + 2] = (byte)(color.Red * 255);
                backBuffer[index4 + 3] = (byte)(color.Alpha * 255);
            }
        }

        // Called to put a pixel on screen at a specific X,Y coordinates
        public void PutPixel2D(int x, int y, Color4 color)
        {
            // As we have a 1-D Array for our back buffer
            // we need to know the equivalent cell in 1-D based
            // on the 2D coordinates on screen
            var index = (x + y * bmp.PixelWidth) * 4;

            backBuffer[index] = (byte)(color.Blue * 255);
            backBuffer[index + 1] = (byte)(color.Green * 255);
            backBuffer[index + 2] = (byte)(color.Red * 255);
            backBuffer[index + 3] = (byte)(color.Alpha * 255);
        }

        // This method is called to clear the back buffer with a specific color
        public void Clear(byte r, byte g, byte b, byte a)
        {
            // Clearing Back Buffer
            for (var index = 0; index < backBuffer.Length; index += 4)
            {
                // BGRA is used by Windows instead by RGBA in HTML5
                backBuffer[index] = b;
                backBuffer[index + 1] = g;
                backBuffer[index + 2] = r;
                backBuffer[index + 3] = a;
            }

            // Clearing Depth Buffer
            for (var index = 0; index < depthBuffer.Length; index++)
            {
                depthBuffer[index] = float.MaxValue;
            }
        }

        // DrawPoint calls PutPixel but does the clipping operation before
        public void DrawPoint(Vector3 point, Color4 color)
        {
            // Clipping what's visible on screen
            if (point.X >= 0 && point.Y >= 0 && point.X < renderWidth && point.Y < renderHeight)
            {
                // Drawing a point
                PutPixel((int)point.X, (int)point.Y, point.Z, color);
            }
        }

        public void DrawPoint2D(Vector2 point, Color4 color)
        {
            // Clipping what's visible on screen
            if (point.X >= 0 && point.Y >= 0 && point.X < bmp.PixelWidth && point.Y < bmp.PixelHeight)
            {
                // Drawing a yellow point
                PutPixel2D((int)point.X, (int)point.Y, color);
            }
        }

        public void DrawBline(Vector2 point0, Vector2 point1, Color4 color)
        {
            int x0 = (int)point0.X;
            int y0 = (int)point0.Y;
            int x1 = (int)point1.X;
            int y1 = (int)point1.Y;

            var dx = Math.Abs(x1 - x0);
            var dy = Math.Abs(y1 - y0);
            var sx = (x0 < x1) ? 1 : -1;
            var sy = (y0 < y1) ? 1 : -1;
            var err = dx - dy;

            while (true)
            {
                DrawPoint2D(new Vector2(x0, y0), color);

                if ((x0 == x1) && (y0 == y1)) break;
                var e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x0 += sx; }
                if (e2 < dx) { err += dx; y0 += sy; }
            }
        }

        // Once everything is ready, we can flush the back buffer
        // into the front buffer. 
        public void Present()
        {
            using (var stream = bmp.PixelBuffer.AsStream())
            {
                // writing our byte[] back buffer into our WriteableBitmap stream
                stream.Write(backBuffer, 0, backBuffer.Length);
            }
            // request a redraw of the entire bitmap
            bmp.Invalidate();
        }

        // Clamping values to keep them between 0 and 1
        float Clamp(float value, float min = 0, float max = 1)
        {
            return Math.Max(min, Math.Min(value, max));
        }

        // Interpolating the value between 2 vertices 
        // min is the starting point, max the ending point
        // and gradient the % between the 2 points
        float Interpolate(float min, float max, float gradient)
        {
            return min + (max - min) * Clamp(gradient);
        }


        // Project takes some 3D coordinates and transform them
        // in 2D coordinates using the transformation matrix
        // It also transform the same coordinates and the normal to the vertex 
        // in the 3D world
        public Vertex Project(Vertex vertex, Matrix transMat, Matrix world)
        {
            // transforming the coordinates into 2D space
            var point2d = Vector3.TransformCoordinate(vertex.Coordinates, transMat);
            // transforming the coordinates & the normal to the vertex in the 3D world
            var point3dWorld = Vector3.TransformCoordinate(vertex.Coordinates, world);
            var normal3dWorld = Vector3.TransformCoordinate(vertex.Normal, world);

            // The transformed coordinates will be based on coordinate system
            // starting on the center of the screen. But drawing on screen normally starts
            // from top left. We then need to transform them again to have x:0, y:0 on top left.
            var x = point2d.X * renderWidth + renderWidth / 2.0f;
            var y = -point2d.Y * renderHeight + renderHeight / 2.0f;

            return new Vertex
            {
                Coordinates = new Vector3(x, y, point2d.Z),
                Normal = normal3dWorld,
                WorldCoordinates = point3dWorld,
                TextureCoordinates = vertex.TextureCoordinates
            };
        }
        public Vector2 Project2D(Vector3 coord, Matrix transMat)
        {
            // transforming the coordinates
            var point = Vector3.TransformCoordinate(coord, transMat);
            // The transformed coordinates will be based on coordinate system
            // starting on the center of the screen. But drawing on screen normally starts
            // from top left. We then need to transform them again to have x:0, y:0 on top left.
            var x = point.X * bmp.PixelWidth + bmp.PixelWidth / 2.0f;
            var y = -point.Y * bmp.PixelHeight + bmp.PixelHeight / 2.0f;
            return (new Vector2(x, y));
        }


        // drawing line between 2 points from left to right
        // papb -> pcpd
        // pa, pb, pc, pd must then be sorted before
        void ProcessScanLine(ScanLineData data, Vertex va, Vertex vb, Vertex vc, Vertex vd, Color4 color, Texture texture)
        {
            Vector3 pa = va.Coordinates;
            Vector3 pb = vb.Coordinates;
            Vector3 pc = vc.Coordinates;
            Vector3 pd = vd.Coordinates;

            // Thanks to current Y, we can compute the gradient to compute others values like
            // the starting X (sx) and ending X (ex) to draw between
            // if pa.Y == pb.Y or pc.Y == pd.Y, gradient is forced to 1
            var gradient1 = pa.Y != pb.Y ? (data.currentY - pa.Y) / (pb.Y - pa.Y) : 1;
            var gradient2 = pc.Y != pd.Y ? (data.currentY - pc.Y) / (pd.Y - pc.Y) : 1;

            int sx = (int)Interpolate(pa.X, pb.X, gradient1);
            int ex = (int)Interpolate(pc.X, pd.X, gradient2);

            // starting Z & ending Z
            float z1 = Interpolate(pa.Z, pb.Z, gradient1);
            float z2 = Interpolate(pc.Z, pd.Z, gradient2);

            // Interpolating normals on Y
            var snl = Interpolate(data.ndotla, data.ndotlb, gradient1);
            var enl = Interpolate(data.ndotlc, data.ndotld, gradient2);

            // Interpolating texture coordinates on Y
            var su = Interpolate(data.ua, data.ub, gradient1);
            var eu = Interpolate(data.uc, data.ud, gradient2);
            var sv = Interpolate(data.va, data.vb, gradient1);
            var ev = Interpolate(data.vc, data.vd, gradient2);

            // drawing a line from left (sx) to right (ex) 
            for (var x = sx; x < ex; x++)
            {
                float gradient = (x - sx) / (float)(ex - sx);

                // Interpolating Z, normal and texture coordinates on X
                var z = Interpolate(z1, z2, gradient);
                var ndotl = Interpolate(snl, enl, gradient);
                var u = Interpolate(su, eu, gradient);
                var v = Interpolate(sv, ev, gradient);

                Color4 textureColor;

                if (texture != null)
                    textureColor = texture.Map(u, v);
                else
                    textureColor = new Color4(1, 1, 1, 1);

                // changing the native color value using the cosine of the angle
                // between the light vector and the normal vector
                // and the texture color
                DrawPoint(new Vector3(x, data.currentY, z), color * ndotl * textureColor);
            }
        }

        // Compute the cosine of the angle between the light vector and the normal vector
        // Returns a value between 0 and 1
        float ComputeNDotL(Vector3 vertex, Vector3 normal, Vector3 lightPosition)
        {
            var lightDirection = lightPosition - vertex;

            normal.Normalize();
            lightDirection.Normalize();

            return Math.Max(0, Vector3.Dot(normal, lightDirection));
        }

        public void DrawTriangle(Vertex v1, Vertex v2, Vertex v3, Color4 color, Texture texture)
        {
            // Sorting the points in order to always have this order on screen p1, p2 & p3
            // with p1 always up (thus having the Y the lowest possible to be near the top screen)
            // then p2 between p1 & p3
            if (v1.Coordinates.Y > v2.Coordinates.Y)
            {
                var temp = v2;
                v2 = v1;
                v1 = temp;
            }

            if (v2.Coordinates.Y > v3.Coordinates.Y)
            {
                var temp = v2;
                v2 = v3;
                v3 = temp;
            }

            if (v1.Coordinates.Y > v2.Coordinates.Y)
            {
                var temp = v2;
                v2 = v1;
                v1 = temp;
            }

            Vector3 p1 = v1.Coordinates;
            Vector3 p2 = v2.Coordinates;
            Vector3 p3 = v3.Coordinates;

            Vector3 centerPoint = (v1.WorldCoordinates + v2.WorldCoordinates + v3.WorldCoordinates) / 3;

            // computing the cos of the angle between the light vector and the normal vector
            // it will return a value between 0 and 1 that will be used as the intensity of the color

            Light light = new Light(new Vector3(float.MaxValue, float.MaxValue, float.MaxValue), 0f, 0f);
            var distance = float.MaxValue;

            foreach (Light l in Lights)
            {
                var temporaryDistance = Vector3.Distance(l.Position, centerPoint);
                if (distance > temporaryDistance && temporaryDistance < l.Range)
                {
                    distance = temporaryDistance;
                    light = l;
                }
            }

            // computing the cos of the angle between the light vector and the normal vector
            // it will return a value between 0 and 1 that will be used as the intensity of the color
            float nl1 = ComputeNDotL(v1.WorldCoordinates, v1.Normal, light.Position);
            float nl2 = ComputeNDotL(v2.WorldCoordinates, v2.Normal, light.Position);
            float nl3 = ComputeNDotL(v3.WorldCoordinates, v3.Normal, light.Position);
            var data = new ScanLineData { };

            // First case where triangles are like that:
            // P1
            // -
            // -- 
            // - -
            // -  -
            // -   - P2
            // -  -
            // - -
            // -
            // P3
            if (LineSide2D(p2, p1, p3) > 0)
            {
                for (var y = (int)p1.Y; y <= (int)p3.Y; y++)
                {
                    data.currentY = y;

                    if (y < p2.Y)
                    {
                        data.ndotla = nl1;
                        data.ndotlb = nl3;
                        data.ndotlc = nl1;
                        data.ndotld = nl2;

                        data.ua = v1.TextureCoordinates.X;
                        data.ub = v3.TextureCoordinates.X;
                        data.uc = v1.TextureCoordinates.X;
                        data.ud = v2.TextureCoordinates.X;

                        data.va = v1.TextureCoordinates.Y;
                        data.vb = v3.TextureCoordinates.Y;
                        data.vc = v1.TextureCoordinates.Y;
                        data.vd = v2.TextureCoordinates.Y;

                        ProcessScanLine(data, v1, v3, v1, v2, new Color4(color.Red * light.RIntesnsity, color.Green * light.GIntesnsity, color.Blue * light.BIntesnsity, color.Alpha * light.AIntensity) * Math.Max(light.Intensity / (distance / 2), 1f), texture);
                    }
                    else
                    {
                        data.ndotla = nl1;
                        data.ndotlb = nl3;
                        data.ndotlc = nl2;
                        data.ndotld = nl3;

                        data.ua = v1.TextureCoordinates.X;
                        data.ub = v3.TextureCoordinates.X;
                        data.uc = v2.TextureCoordinates.X;
                        data.ud = v3.TextureCoordinates.X;

                        data.va = v1.TextureCoordinates.Y;
                        data.vb = v3.TextureCoordinates.Y;
                        data.vc = v2.TextureCoordinates.Y;
                        data.vd = v3.TextureCoordinates.Y;

                        ProcessScanLine(data, v1, v3, v2, v3, new Color4(color.Red * light.RIntesnsity, color.Green * light.GIntesnsity, color.Blue * light.BIntesnsity, color.Alpha * light.AIntensity) * Math.Max(light.Intensity / (distance / 2), 1f), texture);
                    }
                }
            }
            // First case where triangles are like that:
            //       P1
            //        -
            //       -- 
            //      - -
            //     -  -
            // P2 -   - 
            //     -  -
            //      - -
            //        -
            //       P3
            else
            {
                for (var y = (int)p1.Y; y <= (int)p3.Y; y++)
                {
                    data.currentY = y;

                    if (y < p2.Y)
                    {
                        data.ndotla = nl1;
                        data.ndotlb = nl2;
                        data.ndotlc = nl1;
                        data.ndotld = nl3;

                        data.ua = v1.TextureCoordinates.X;
                        data.ub = v2.TextureCoordinates.X;
                        data.uc = v1.TextureCoordinates.X;
                        data.ud = v3.TextureCoordinates.X;

                        data.va = v1.TextureCoordinates.Y;
                        data.vb = v2.TextureCoordinates.Y;
                        data.vc = v1.TextureCoordinates.Y;
                        data.vd = v3.TextureCoordinates.Y;

                        ProcessScanLine(data, v1, v2, v1, v3, new Color4(color.Red * light.RIntesnsity, color.Green * light.GIntesnsity, color.Blue * light.BIntesnsity, color.Alpha * light.AIntensity) * Math.Max(light.Intensity / (distance / 2), 1f), texture);
                    }
                    else
                    {
                        data.ndotla = nl2;
                        data.ndotlb = nl3;
                        data.ndotlc = nl1;
                        data.ndotld = nl3;

                        data.ua = v2.TextureCoordinates.X;
                        data.ub = v3.TextureCoordinates.X;
                        data.uc = v1.TextureCoordinates.X;
                        data.ud = v3.TextureCoordinates.X;

                        data.va = v2.TextureCoordinates.Y;
                        data.vb = v3.TextureCoordinates.Y;
                        data.vc = v1.TextureCoordinates.Y;
                        data.vd = v3.TextureCoordinates.Y;

                        ProcessScanLine(data, v2, v3, v1, v3, new Color4(color.Red * light.RIntesnsity, color.Green * light.GIntesnsity, color.Blue * light.BIntesnsity, color.Alpha * light.AIntensity) * Math.Max(light.Intensity / (distance / 2), 1f), texture);
                    }
                }
            }
        }


        // The main method of the engine that re-compute each vertex projection
        // during each frame
        // The main method of the engine that re-compute each vertex projection
        // during each frame
        public void Render(Camera camera, params Mesh[] meshes)
        {
            // To understand this part, please read the prerequisites resources
            var viewMatrix = Matrix.LookAtLH(camera.Position, camera.Target, Vector3.UnitY);
            var projectionMatrix = Matrix.PerspectiveFovLH(0.78f,
                                                           (float)renderWidth / renderHeight,
                                                           0.01f, 1.0f);

            foreach (Mesh mesh in meshes)
            {
                // Beware to apply rotation before translation 
                var worldMatrix = Matrix.RotationYawPitchRoll(mesh.Rotation.Y, mesh.Rotation.X, mesh.Rotation.Z) *
                                  Matrix.Translation(mesh.Position);

                var worldView = worldMatrix * viewMatrix;
                var transformMatrix = worldView * projectionMatrix;

                Parallel.For(0, mesh.Faces.Length, faceIndex =>
                {
                    var face = mesh.Faces[faceIndex];

                    // Render this face
                    var vertexA = mesh.Vertices[face.A];
                    var vertexB = mesh.Vertices[face.B];
                    var vertexC = mesh.Vertices[face.C];

                    var pixelA = Project(vertexA, transformMatrix, worldMatrix);
                    var pixelB = Project(vertexB, transformMatrix, worldMatrix);
                    var pixelC = Project(vertexC, transformMatrix, worldMatrix);

                    //var color = 0.25f + (faceIndex % mesh.Faces.Length) * 0.75f / mesh.Faces.Length;
                    var color = 1.0f;
                    DrawTriangle(pixelA, pixelB, pixelC, new Color4(color, color, color, 1), mesh.Texture);
                });
            }
        }

        // Loading the JSON file in an asynchronous manner
        public async Task<Mesh[]> LoadJSONFileAsync(string fileName, Mesh[] ListOfMeshes, bool grav, string tFilePath, int width, int height)
        {
            var meshes = ListOfMeshes.ToList<Mesh>();
            var materials = new Dictionary<String, Material>();
            var file = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(fileName);
            var data = await Windows.Storage.FileIO.ReadTextAsync(file);
            dynamic jsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject(data);

            for (var materialIndex = 0; materialIndex < jsonObject.materials.Count; materialIndex++)
            {
                var material = new Material();
                material.Name = jsonObject.materials[materialIndex].name.Value;
                material.ID = jsonObject.materials[materialIndex].id.Value;
                if (jsonObject.materials[materialIndex].diffuseTexture != null)
                    material.DiffuseTextureName = jsonObject.materials[materialIndex].diffuseTexture.name.Value;

                materials.Add(material.ID, material);
            }

            for (var meshIndex = 0; meshIndex < jsonObject.meshes.Count; meshIndex++)
            {
                var verticesArray = jsonObject.meshes[meshIndex].vertices;
                // Faces
                var indicesArray = jsonObject.meshes[meshIndex].indices;

                var uvCount = jsonObject.meshes[meshIndex].uvCount.Value;
                var verticesStep = 1;

                // Depending of the number of texture's coordinates per vertex
                // we're jumping in the vertices array  by 6, 8 & 10 windows frame
                switch ((int)uvCount)
                {
                    case 0:
                        verticesStep = 6;
                        break;
                    case 1:
                        verticesStep = 8;
                        break;
                    case 2:
                        verticesStep = 10;
                        break;
                }

                // the number of interesting vertices information for us
                var verticesCount = verticesArray.Count / verticesStep;
                // number of faces is logically the size of the array divided by 3 (A, B, C)
                var facesCount = indicesArray.Count / 3;
                var mesh = new Mesh(jsonObject.meshes[meshIndex].name.Value, verticesCount, facesCount, new PhysicsItem(grav, 1f));

                // Filling the Vertices array of our mesh first
                for (var index = 0; index < verticesCount; index++)
                {
                    var x = (float)verticesArray[index * verticesStep].Value;
                    var y = (float)verticesArray[index * verticesStep + 1].Value;
                    var z = (float)verticesArray[index * verticesStep + 2].Value;
                    // Loading the vertex normal exported by Blender
                    var nx = (float)verticesArray[index * verticesStep + 3].Value;
                    var ny = (float)verticesArray[index * verticesStep + 4].Value;
                    var nz = (float)verticesArray[index * verticesStep + 5].Value;

                    mesh.Vertices[index] = new Vertex
                    {
                        Coordinates = new Vector3(x, y, z),
                        Normal = new Vector3(nx, ny, nz)
                    };

                    if (uvCount > 0)
                    {
                        // Loading the texture coordinates
                        float u = (float)verticesArray[index * verticesStep + 6].Value;
                        float v = (float)verticesArray[index * verticesStep + 7].Value;
                        mesh.Vertices[index].TextureCoordinates = new Vector2(u, v);
                    }
                }

                // Then filling the Faces array
                for (var index = 0; index < facesCount; index++)
                {
                    var a = (int)indicesArray[index * 3].Value;
                    var b = (int)indicesArray[index * 3 + 1].Value;
                    var c = (int)indicesArray[index * 3 + 2].Value;
                    mesh.Faces[index] = new Face { A = a, B = b, C = c };
                }

                // Getting the position you've set in Blender
                var position = jsonObject.meshes[meshIndex].position;
                mesh.Position = new Vector3((float)position[0].Value, (float)position[1].Value, (float)position[2].Value);

                if (uvCount > 0)
                {
                    // Texture
                    var meshTextureID = jsonObject.meshes[meshIndex].materialId.Value;
                    var meshTextureName = materials[meshTextureID].DiffuseTextureName;
                    mesh.Texture = new Texture(tFilePath, width, height);
                }

                meshes.Add(mesh);
            }
            return meshes.ToArray();
        }

        public Vector3[] Bresenham3D(float x1, float y1, float z1, float x2, float y2, float z2)
        {
            var ListOfPoints = new List<Vector3>();
            ListOfPoints.Add(new Vector3(x1, y1, z1));
            int xs = 0;
            int ys = 0;
            int zs = 0;
            var dx = Math.Abs(x2 - x1);
            var dy = Math.Abs(y2 - y1);
            var dz = Math.Abs(z2 - z1);
            if (x2 > x1)
            {
                xs = 1;
            }
            else
            {
                xs = -1;
            }
            if (y2 > y1)
            {
                ys = 1;
            }
            else
            {
                ys = -1;
            }
            if (z2 > z1)
            {
                zs = 1;
            }
            else
            {
                zs = -1;
            }
            // Driving axis is X-axis"
            if (dx >= dy && dx >= dz)
            {
                var p1 = 2 * dy - dx;
                var p2 = 2 * dz - dx;
                while (x1 != x2)
                {
                    x1 += xs;
                    if (p1 >= 0)
                    {
                        y1 += ys;
                        p1 -= 2 * dx;
                    }
                    if (p2 >= 0)
                    {
                        z1 += zs;
                        p2 -= 2 * dx;
                    }
                    p1 += 2 * dy;
                    p2 += 2 * dz;
                    ListOfPoints.Add(new Vector3(x1, y1, z1));
                }
            }
            else if (dy >= dx && dy >= dz)
            {
                // Driving axis is Y-axis"
                var p1 = 2 * dx - dy;
                var p2 = 2 * dz - dy;
                while (y1 != y2)
                {
                    y1 += ys;
                    if (p1 >= 0)
                    {
                        x1 += xs;
                        p1 -= 2 * dy;
                    }
                    if (p2 >= 0)
                    {
                        z1 += zs;
                        p2 -= 2 * dy;
                    }
                    p1 += 2 * dx;
                    p2 += 2 * dz;
                    ListOfPoints.Add(new Vector3(x1, y1, z1));
                }
            }
            else
            {
                // Driving axis is Z-axis"
                var p1 = 2 * dy - dz;
                var p2 = 2 * dx - dz;
                while (z1 != z2)
                {
                    z1 += zs;
                    if (p1 >= 0)
                    {
                        y1 += ys;
                        p1 -= 2 * dz;
                    }
                    if (p2 >= 0)
                    {
                        x1 += xs;
                        p2 -= 2 * dz;
                    }
                    p1 += 2 * dy;
                    p2 += 2 * dx;
                    ListOfPoints.Add(new Vector3(x1, y1, z1));
                }
            }

            return ListOfPoints.ToArray();
        }


        // 2D vector cross product – uses only X and Y coordinates, ignores Z

        float Cross2D(float x0, float y0, float x1, float y1)
        {
            return x0 * y1 - x1* y0;
        }

        float LineSide2D(Vector3 p, Vector3 lineFrom, Vector3 lineTo)
        {
            return Cross2D(p.X - lineFrom.X, p.Y - lineFrom.Y, lineTo.X - lineFrom.X, lineTo.Y - lineFrom.Y);
        }
    }
}
