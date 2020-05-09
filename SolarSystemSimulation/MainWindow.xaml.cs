using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Utilities;
using SharpDX;
using DiffuseMaterial = HelixToolkit.Wpf.SharpDX.DiffuseMaterial;
using OrthographicCamera = HelixToolkit.Wpf.SharpDX.OrthographicCamera;

namespace SolarSystemSimulation
{
    public partial class MainWindow
    {
        static NVOptimusEnabler nvEnabler = new NVOptimusEnabler();

        public EffectsManager EffectsManager { get; }
        public OrthographicCamera Camera { get; }

        public Color4 LightColor { get; }

        public Stream EnvironmentMap { get; set; }

        private SolarSystem.SolarSystem _system;

        public static MemoryStream LoadFileToMemory(string filepath)
        {
            using (var fs = new FileStream(filepath, FileMode.Open))
            {
                var ms = new MemoryStream();
                fs.CopyTo(ms);
                return ms;
            }
        }

        public MainWindow()
        {
            _system = new SolarSystem.SolarSystem();
            EnvironmentMap = LoadFileToMemory(@"Resources\skymap.dds");

            Console.WriteLine(EnvironmentMap.Length);

            LightColor = new Color {R = 255, G = 255, B = 255, A = 155};
            EffectsManager = new DefaultEffectsManager();
            Camera = new OrthographicCamera();

            var sun = _system.Bodies[0];
            sun.Material = new DiffuseMaterial
            {
                DiffuseMap = LoadFileToMemory(@"Resources\sun.jpg")
            };

            InitializeComponent();

            Loaded += (sender, args) => InitCamera();
            Viewport3DX.SizeChanged += OnSizeChanged;

            _system.Bodies.ForEach(Viewport3DX.Items.Add);

            Task.Run(() => _system.Update(8));
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Camera.Width = e.NewSize.Width;
        }

        private void InitCamera()
        {
            Camera.LookAt(new Point3D(0, 0, 0),
                new Vector3D(-4000, -4000, -4000),
                0);
            Camera.FarPlaneDistance = 1e5;
            Camera.NearPlaneDistance = 0;

            Viewport3DX.CameraMode = CameraMode.Inspect;
            Viewport3DX.ShowFrameRate = true;
        }
    }
}