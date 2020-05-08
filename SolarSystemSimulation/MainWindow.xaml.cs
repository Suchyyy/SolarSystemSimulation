using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Utilities;
using SolarSystemSimulation.SolarSystem;
using SolarSystemSimulation.Texture;
using OrthographicCamera = HelixToolkit.Wpf.SharpDX.OrthographicCamera;

namespace SolarSystemSimulation
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        static NVOptimusEnabler nvEnabler = new NVOptimusEnabler();

        public EffectsManager EffectsManager { get; }
        public OrthographicCamera Camera { get; }

        public Color LightColor { get; }

        private List<AstronomicalObject> objects;

        public MainWindow()
        {
            LightColor = new Color {ScR = 1, ScG = 1, ScB = 1, ScA = 0.9f};
            EffectsManager = new DefaultEffectsManager();
            Camera = new OrthographicCamera();

            InitializeComponent();

            Loaded += (sender, args) => InitCamera();
            Viewport3D.SizeChanged += OnSizeChanged;

            AddPlanets();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Camera.Width = e.NewSize.Width;
        }

        private void AddPlanets()
        {
            var sun = new AstronomicalObject(109)
            {
                Mass = 333_000 * AstronomicalObject.Me,
                Position = new Point3D(0, 0, 0),
                Velocity = new Vector3D(0, 0, 0),
                Material = TextureHelper.LoadTexture("SolarSystemSimulation.Resources.sun.jpg")
            };
            var earth = new AstronomicalObject(11)
            {
                Mass = AstronomicalObject.Me,
                Position = new Point3D(200, 0, 0), /* 1 au - 1.49 * 10^11 m */
                Velocity = new Vector3D(0, 0, -30_000), /* avg 30 km/s */
                Material = TextureHelper.LoadTexture("SolarSystemSimulation.Resources.earth.jpg"),
                Transform = new TranslateTransform3D(200, 0, 0)
            };

            objects = new List<AstronomicalObject> {sun, earth};

            // Viewport3D.Children.Add(skybox);
            Viewport3D.Items.Add(sun);
            Viewport3D.Items.Add(earth);
        }

        private void InitCamera()
        {
            Camera.LookAt(new Point3D(0, 0, 0),
                new Vector3D(-100, -100, -100),
                0);
            Camera.FarPlaneDistance = AstronomicalObject.Au * 1e4;

            Viewport3D.CameraMode = CameraMode.Inspect;
        }

        private void Tick()
        {
            var forces = objects.Select(o => o.GetGravity(objects)).ToArray();

            for (var i = 0; i < forces.Length; i++)
            {
                var o = objects[i];
                o.Velocity += forces[i] / o.Mass;
            }

            objects.ForEach(o => o.UpdatePosition());
        }

        // private void Render()
        // {
        //     foreach (var astronomicalObject in objects)
        //         astronomicalObject.Dispatcher?.Invoke(() =>
        //         {
        //             astronomicalObject.Transform =
        //                 new TranslateTransform3D(astronomicalObject.Position.ToVector3D());
        //         });
        // }
    }
}