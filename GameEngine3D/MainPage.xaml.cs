using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace GameEngine3D
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Device device;
        private Mesh[] meshes;
        Camera mera = new Camera();
        public List<Light> lights = new List<Light>();
        Player player = new Player(Vector3.Zero, Vector3.Zero, 10f);

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Light lightTemp = new Light(new Vector3(0f, 0f, 10f), 5f, 50f);
            Light lightTemp2 = new Light(new Vector3(-50, 0f, 10f), 5f, 50f);
            lights.Add(lightTemp);
            lights.Add(lightTemp2);
            // Choose the back buffer resolution here
            WriteableBitmap bmp = new WriteableBitmap(640, 480);

            // Our Image XAML control
            frontBuffer.Source = bmp;

            device = new Device(bmp);
            
            meshes = await device.LoadJSONFileAsync(@"Assets\Mesh\ZICO.babylon", meshes);
            meshes[0].Position += new Vector3(-3f, 0f, 0f);
            meshes = await device.LoadJSONFileAsync(@"Assets\Mesh\MONKE.babylon", meshes);
            meshes[1].Rotation += new Vector3(0f, 0f, 0f);

            // Registering to the XAML rendering loop
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        // Rendering loop handler
        void CompositionTarget_Rendering(object sender, object e)
        {
            device.Clear(0, 0, 0, 255);

            Update();

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
            mera.Position = player.Position;
            mera.Target = player.Rotation;

            foreach (var mesh in meshes)
            {
                // rotating slightly the meshes during each frame rendered
                mesh.Rotation = new Vector3(mesh.Rotation.X, mesh.Rotation.Y + 0.02f, mesh.Rotation.Z);
            }
        }
    }
}
