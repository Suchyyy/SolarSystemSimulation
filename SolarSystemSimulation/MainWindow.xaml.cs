using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Utilities;
using SharpDX;
using SolarSystemSimulation.SolarSystem;
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

        private readonly SolarSystem.SolarSystem _system;

        public int Planets { get; set; } = 5;
        public int SimulationTime { get; set; } = 10;

        public MainWindow()
        {
            _system = new SolarSystem.SolarSystem();
            EnvironmentMap = TextureHelper.LoadFileToMemory(@"Resources\skymap.dds");

            LightColor = new Color {R = 255, G = 255, B = 255, A = 155};
            EffectsManager = new DefaultEffectsManager();
            Camera = new OrthographicCamera();

            InitializeComponent();

            Loaded += (sender, args) => InitCamera();
            Viewport3DX.SizeChanged += OnSizeChanged;

            _system.Bodies.ForEach(Viewport3DX.Items.Add);

            var meshBuilder = new MeshBuilder(true, true, true);
            meshBuilder.AddPipes(new List<Vector3>
            {
                new Vector3(0, 0, 0),
                new Vector3(500, 0, 0),
                new Vector3(500, 500, 0),
                new Vector3(500, 500, 500)
            }, new List<int> {0, 1, 1, 2, 2, 3}, 100);
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
            Camera.FarPlaneDistance = AstronomicalObject.Au;
            Camera.NearPlaneDistance = 0;

            Viewport3DX.CameraMode = CameraMode.Inspect;
        }

        private void StartSimulation_OnClick(object sender, RoutedEventArgs e)
        {
            var but = (Button) sender;
            but.IsEnabled = false;

            Task.Run(() => _system.StartSimulation(60, 16));
            Task.Delay(TimeSpan.FromSeconds(SimulationTime)).ContinueWith(_ =>
            {
                _system.IsRunning = false;
                but.Dispatcher?.Invoke(() => but.IsEnabled = true);

                Application.Current.Dispatcher?.Invoke(() =>
                {
                    new OrbitWindow(_system.Orbits, 1).Show();
                    _system.Orbits.Clear();
                });
            });
        }
    }
}