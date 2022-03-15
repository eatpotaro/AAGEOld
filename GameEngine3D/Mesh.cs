using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using Windows.UI.Xaml.Media.Imaging;
using SoftEngine;

namespace GameEngine3D
{
    public struct Face
    {
        public int A;
        public int B;
        public int C;
    }

    public class Mesh
    {
        public static List<Mesh> Meshes { get; set; }
        public string Name { get; set; }
        public Vertex[] Vertices { get; private set; }
        public Face[] Faces { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public PhysicsItem Physics { get; set; }
        public BoxCollider Collider { get; set; }
        public Texture Texture { get; set; }

        public Mesh(string name, int verticesCount, int facesCount, PhysicsItem p)
        {
            if (Meshes == null) Meshes = new List<Mesh>();
            Vertices = new Vertex[verticesCount];
            Faces = new Face[facesCount];
            Name = name;
            Physics = p;
            p.Mesh = this;
            Collider = new BoxCollider(this);
            Meshes.Add(this);
        }
    }

    public struct Vertex
    {
        public Vector3 Normal;
        public Vector3 Coordinates;
        public Vector3 WorldCoordinates;
        public Vector2 TextureCoordinates;
    }
}
