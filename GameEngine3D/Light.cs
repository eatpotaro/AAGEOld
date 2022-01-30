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
        public float RIntesnsity { get; set; }
        public float GIntesnsity { get; set; }
        public float BIntesnsity { get; set; }
        public float AIntensity { get; set; }

        public Light(Vector3 Pos)
        {
            Position = new Vector3(0f, 0f, 0f);
            Position = Pos;
            Intensity = 50f;
            Range = 10f;
            RIntesnsity = 1f;
            GIntesnsity = 1f;
            BIntesnsity = 1f;
            AIntensity = 1f;
        }
        public Light(Vector3 Pos, float Intense)
        {
            Position = new Vector3(0f, 0f, 0f);
            Position = Pos;
            Intensity = Intense;
            Range = 10f;
            RIntesnsity = 1f;
            GIntesnsity = 1f;
            BIntesnsity = 1f;
            AIntensity = 1f;
        }
        public Light(Vector3 Pos, float Intense, float TempRange)
        {
            Position = new Vector3(0f, 0f, 0f);
            Position = Pos;
            Intensity = Intense;
            Range = TempRange;
            RIntesnsity = 1f;
            GIntesnsity = 1f;
            BIntesnsity = 1f;
            AIntensity = 1f;
        }
        public Light(Vector3 Pos, float Intense, float TempRange, float r, float g, float b, float a)
        {
            Position = new Vector3(0f, 0f, 0f);
            Position = Pos;
            Intensity = Intense;
            Range = TempRange;
            RIntesnsity = r;
            GIntesnsity = g;
            BIntesnsity = b;
            AIntensity = a;
        }
    }
}