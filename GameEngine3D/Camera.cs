using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using Windows.UI.Xaml.Media.Imaging;

namespace GameEngine3D
{
    public class Camera
    {
        public Vector3 Position { get; set; }
        public Vector3 Target { get; set; }

        public Vector3 CamToWorld(Vector3 Camera)
        {
            return Camera;
        }
    }
}
