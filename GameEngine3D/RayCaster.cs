using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine3D
{
    class RayCaster
    {

    }

    public class Ray
    {
        public Vector3 Position { get; set; }
        public float Length { get; set; }
        public int ChecksPerUnit { get; set; }
        public Vector3 Direction { get; set; }

        public bool Collides { get; set; }
        public float Magnitude { get; set; }
        public Mesh Collision { get; set; }

        public Ray(Vector3 pos, float Length, int Checks, Vector3 Dir)
        {
            this.Position = pos;
            this.Length = Length;
            this.ChecksPerUnit = Checks;
            this.Direction = Dir;
        }

        public void Casts(Ray r, List<Mesh> m)
        {
            Collides = false;
            for (float i = 0; i < Length; i += 1f / ChecksPerUnit)
            {
                Vector3 CheckPos = r.Position + (Direction * i);
                foreach(Mesh mesh in m)
                {
                    mesh.Collider.MinPos += mesh.Position;
                    mesh.Collider.MaxPos += mesh.Position;

                    if (mesh.Collider.PointIntersect(CheckPos, mesh.Collider))
                    {
                        Collides = true;
                        Magnitude = i;
                        Collision = mesh;

                        mesh.Collider.UpdateSize(mesh);
                        return;
                    }

                    mesh.Collider.UpdateSize(mesh);

                }
            }
        }
    }
}
