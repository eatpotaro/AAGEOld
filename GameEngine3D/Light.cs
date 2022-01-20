using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine3D
{
    public class Light
    {
        public Vector3 Position { get; set; }
        public float Intensity { get; set; }
        public float Range { get; set; }

        public Light(Vector3 Pos)
        {
            Position = new Vector3(0f, 0f, 0f);
            Position = Pos;
            Intensity = 50f;
        }
        public Light(Vector3 Pos, float Intense)
        {
            Position = new Vector3(0f, 0f, 0f);
            Position = Pos;
            Intensity = Intense;
        }
        public Light(Vector3 Pos, float Intense, float TempRange)
        {
            Position = new Vector3(0f, 0f, 0f);
            Position = Pos;
            Intensity = Intense;
            Range = TempRange;
        }
    }
}
