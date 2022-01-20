using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine3D
{
    public class Player
    {
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public float Speed { get; set; }

        public Player(Vector3 pos, Vector3 rot, float speed)
        {
            Position = pos;
            Rotation = rot;
            Speed = speed;
        }
    }
}
