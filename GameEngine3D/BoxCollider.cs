using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace GameEngine3D
{
    public class BoxCollider
    {
        public Vector3 MinPos = new Vector3(0, 0, 0);
        public Vector3 MaxPos = new Vector3(0, 0, 0);
        public bool Colliding { get; set; }

        public Mesh ThisMesh { get; set; }

        public BoxCollider(Mesh m)
        {
            ThisMesh = m;
        }

        public void UpdateSize(Mesh m)
        {
            Vector3 TempMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 TempMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            foreach (Vertex v in m.Vertices)
            {
                if (v.Coordinates.X < TempMin.X)
                {
                    TempMin.X = v.Coordinates.X;
                }

                if (v.Coordinates.X > TempMax.X)
                {
                    TempMax.X = v.Coordinates.X;
                }

                if (v.Coordinates.Y < TempMin.Y)
                {
                    TempMin.Y = v.Coordinates.Y;
                }

                if (v.Coordinates.Y > TempMax.Y)
                {
                    TempMax.Y = v.Coordinates.Y;
                }

                if (v.Coordinates.Z < TempMin.Z)
                {
                    TempMin.Z = v.Coordinates.Z;
                }

                if (v.Coordinates.Z > TempMax.Z)
                {
                    TempMax.Z = v.Coordinates.Z;
                }
            }

            MinPos = TempMin;
            MaxPos = TempMax;
        }

        public void CheckCollisions(Mesh[] meshes)
        {
            for (int i = 0; i < meshes.Length; i++)
            {
                if (meshes[i].Collider == this)
                {
                    break;
                }

                meshes[i].Collider.MinPos += meshes[i].Position;
                meshes[i].Collider.MaxPos += meshes[i].Position;
                this.MinPos += ThisMesh.Position;
                this.MaxPos += ThisMesh.Position;

                Colliding = false;

                if (Intersect(meshes[i].Collider, this))
                {
                    FixIntersection(meshes[i]);
                    Colliding = true;
                }

                this.UpdateSize(ThisMesh);
                meshes[i].Collider.UpdateSize(meshes[i]);
            }
        }

        private void FixIntersection(Mesh m)
        {
            if(m.Physics.ColliderFixValue == 0)
            {
                return;
            }

            Vector3 Direction = m.Position - ThisMesh.Position;
            //direction from this position to the other mesh position;

            float x = (Direction.X);
            float y = (Direction.Y);
            float z = (Direction.Z);

            float biggest = MathF.Max(MathF.Max(MathF.Abs(y), MathF.Abs(z)), MathF.Abs(x));

            Vector3 Side = new Vector3(0, 0, 0);

            if (biggest == MathF.Abs(x)) Side.X = 1;
            if (biggest == MathF.Abs(y)) Side.Y = 1;
            if (biggest == MathF.Abs(z)) Side.Z = 1;

            float X = m.Position.X;
            float Y = m.Position.Y;
            float Z = m.Position.Z;

            if (Side.X == 1 && Direction.X <= 0)
            {
                X = MinPos.X - (m.Collider.MaxPos.X - m.Position.X);
            }
            else if (Side.X == 1)
            {
                X = MaxPos.X - (m.Collider.MinPos.X - m.Position.X);
                //new posistion of the x for the other mesh is now the maximum position of this mesh;
            }

            if (Side.Y == 1 && Direction.X <= 0)
            {
                Y = MinPos.Y - (m.Collider.MaxPos.Y - m.Position.Y);
            }
            else if (Side.Y == 1)
            {
                Y = MaxPos.Y - (m.Collider.MinPos.Y - m.Position.Y);
                //new posistion of the x for the other mesh is now the maximum position of this mesh;
            }


            if (Side.Z == 1 && Direction.X <= 0)
            {
                Z = MinPos.Z - (m.Collider.MaxPos.Z - m.Position.Z);
            }
            else if (Side.Z == 1)
            {
                Z = MaxPos.Z - (m.Collider.MinPos.Z - m.Position.Z);
                //new posistion of the x for the other mesh is now the maximum position of this mesh;
            }

            m.Position = new Vector3(X, Y, Z);
        }

        public bool Intersect(BoxCollider a, BoxCollider b)
        {
            return ((a.MinPos.X <= b.MaxPos.X && a.MaxPos.X >= b.MinPos.X) && (a.MinPos.Y <= b.MaxPos.Y && a.MaxPos.Y >= b.MinPos.Y) && (a.MinPos.Z <= b.MaxPos.Z && a.MaxPos.Z >= b.MinPos.Z));
        }

        public bool PointIntersect(Vector3 a, BoxCollider b)
        {
            return (a.X < b.MaxPos.X && a.X > b.MinPos.X) && (a.Y < b.MaxPos.Y && a.Y > b.MinPos.Y) && (a.Z < b.MaxPos.Z && a.Z > b.MinPos.Z);
        }
    }
}
