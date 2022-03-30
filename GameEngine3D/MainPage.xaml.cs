using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using System.Windows.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Gaming.Input.Custom;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace GameEngine3D
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Device device;
        private Mesh[] meshes = new Mesh[0];
        Camera mera = new Camera();
        public List<Light> lights = new List<Light>();
        Player player = new Player(Vector3.Zero, Vector3.Zero, 10f);

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Light lightTemp = new Light(new Vector3(0f, 0f, 50f), 1f, 5000000f, 1f, 1f, 1f, 1f);
            //Light lightTemp2 = new Light(new Vector3(-3, 0f, 10f), 1f, 50f, 1f, 1f, 1f, 1f);
            lights.Add(lightTemp);
            //lights.Add(lightTemp2);
            // Choose the back buffer resolution here
            WriteableBitmap bmp = new WriteableBitmap(640, 480);

            // Our Image XAML control
            frontBuffer.Source = bmp;

            device = new Device(bmp);
            
            meshes = await device.LoadJSONFileAsync(@"Assets\Mesh\ZICO.babylon", meshes, false, @"Assets\Textures\Suzanne.jpg", 512, 512, "ball");
            meshes[0].Position += new Vector3(0f, 0f, 0f);
            meshes = await device.LoadJSONFileAsync(@"Assets\Mesh\MONKE2.babylon", meshes, false, @"Assets\Textures\Suzanne.jpg", 512, 512, "m");
            meshes[1].Position += new Vector3(7f, 0f, 0f);
            meshes[1].Physics.ColliderFixValue = 0;
            meshes = await device.LoadJSONFileAsync(@"Assets\Mesh\MONKE2.babylon", meshes, false, @"Assets\Textures\Suzanne.jpg", 512, 512, "m");
            meshes[2].Position += new Vector3(-7f, 0f, 0f);
            meshes[2].Physics.ColliderFixValue = 0;

            player.Position = new Vector3(20f, 0, 40f);
            mera.Position = player.Position;

            // Registering to the XAML rendering loop
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        // Rendering loop handler
        void CompositionTarget_Rendering(object sender, object e)
        {
            device.Clear(60, 60, 60, 255);

            Update();

            foreach (Mesh m in meshes)
            {
                m.Physics.UpdatePhysics();
            }

            // Doing the various matrix operations
            device.Render(mera, meshes);
            // Flushing the back buffer into the front buffer
            device.Present();

            device.Lights = lights;
        }

        public MainPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        void Update()
        {
            Ray ray = new Ray(new Vector3(0, 5, 0), 10, 5, Vector3.Down);
            ray.Casts(ray, Mesh.Meshes);
            if (ray.Collides)
            {
                lights[0].BIntesnsity = 0;
                lights[0].RIntesnsity = 0;
            }
            else
            {
                lights[0].BIntesnsity = 1;
                lights[0].RIntesnsity = 1;
            }

            mera.Position += new Vector3(0f, 0f, 0f);

            if (meshes != null)
            {
                foreach (var mesh in meshes)
                {
                    // rotating slightly the meshes during each frame rendered
                    mesh.Rotation = new Vector3(mesh.Rotation.X, mesh.Rotation.Y + 0.02f, mesh.Rotation.Z);
                }
            }

            if (meshes[0].Position.Y <= -5)
            {
                meshes[0].Position = new Vector3(-3f, 20f, -0f);
            }

            meshes[0].Position += new Vector3(0.05f * (Input.GetKeyDown(Windows.System.VirtualKey.Space) ? -1 : Input.GetKeyDown(Windows.System.VirtualKey.LeftButton) ? 1 : 0), 0, 0);
            meshes[2].Position += new Vector3(0.05f * (Input.GetKeyDown(Windows.System.VirtualKey.A) ? -1 : Input.GetKeyDown(Windows.System.VirtualKey.D) ? 1 : 0), 0, 0);
        }

        void DrawLine(float x0, float y0, float z0, float x1, float y1, float z1, Color4 color)
        {
            var viewMatrix = SharpDX.Matrix.LookAtLH(mera.Position, mera.Target, Vector3.UnitY);
            var projectionMatrix = SharpDX.Matrix.PerspectiveFovRH(0.78f, (float)640 / 480, 0.01f, 1.0f);
            var worldMatrix = SharpDX.Matrix.RotationYawPitchRoll(0, 0, 0) * SharpDX.Matrix.Translation(new Vector3(x0, y0, z0));

            var transformMatrix = worldMatrix * viewMatrix * projectionMatrix;

            Vector2 PixelPlace = device.Project2D(new Vector3(x0, y0, z0), transformMatrix);

            var viewMatrixend = SharpDX.Matrix.LookAtLH(mera.Position, mera.Target, Vector3.UnitY);
            var projectionMatrixend = SharpDX.Matrix.PerspectiveFovRH(0.78f, (float)640 / 480, 0.01f, 1.0f);
            var worldMatrixend = SharpDX.Matrix.RotationYawPitchRoll(0, 0, 0) * SharpDX.Matrix.Translation(new Vector3(x1, y1, z1));

            var transformMatrixend = worldMatrix * viewMatrix * projectionMatrix;

            Vector2 PixelEndPlace = device.Project2D(new Vector3(x1, y1, z1), transformMatrixend);

            device.DrawBline(PixelPlace, PixelEndPlace, color);
        }
        void DrawLine(Vector3 pos0, Vector3 pos1, Color4 color)
        {
            var viewMatrix = SharpDX.Matrix.LookAtLH(mera.Position, mera.Target, Vector3.UnitY);
            var projectionMatrix = SharpDX.Matrix.PerspectiveFovRH(0.78f, (float)640 / 480, 0.01f, 1.0f);
            var worldMatrix = SharpDX.Matrix.RotationYawPitchRoll(0, 0, 0) * SharpDX.Matrix.Translation(pos0);

            var transformMatrix = worldMatrix * viewMatrix * projectionMatrix;

            Vector2 PixelPlace = device.Project2D(pos0, transformMatrix);

            var viewMatrixend = SharpDX.Matrix.LookAtLH(mera.Position, mera.Target, Vector3.UnitY);
            var projectionMatrixend = SharpDX.Matrix.PerspectiveFovRH(0.78f, (float)640 / 480, 0.01f, 1.0f);
            var worldMatrixend = SharpDX.Matrix.RotationYawPitchRoll(0, 0, 0) * SharpDX.Matrix.Translation(pos1);

            var transformMatrixend = worldMatrix * viewMatrix * projectionMatrix;

            Vector2 PixelEndPlace = device.Project2D(pos1, transformMatrixend);

            device.DrawBline(PixelPlace, PixelEndPlace, color);
        }
    }
}
