using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine3D
{
    public class PhysicsItem
    {
        public Mesh Mesh { get; set; }
        public Vector3 Velocity { get; set; }
        private Vector3 GravityVelocity { get; set; }
        public bool UseGravity { get; set; }
        public float Mass { get; set; }
        public float GravityForce { get; set; }
        public float ColliderFixValue { get; set; }
        public PhysicsItem(bool grav, float mass)
        {
            ColliderFixValue = 1f;
            UseGravity = grav;
            Mass = mass;
            GravityForce = -9.81f; 
        }

        private void ApplyForces()
        {
            if (UseGravity) GravityVelocity += new Vector3(0, Mass * GravityForce, 0); else GravityVelocity = Vector3.Zero;

        }

        private void UpdateTransform()
        {
            Velocity += GravityVelocity;                                
            Mesh.Position += Velocity / 600;
        }
        private void UpdateVelocity()
        {
            Velocity = new Vector3(MathF.Sqrt(Velocity.X), MathF.Sqrt(Velocity.Y), MathF.Sqrt(Velocity.Z));
        }

        public void UpdatePhysics()
        {
            ApplyForces();
            UpdateTransform();
            UpdateVelocity();
            Mesh.Collider.UpdateSize(Mesh);
            Mesh.Collider.CheckCollisions(Mesh.Meshes.ToArray());
        }
    }
}
