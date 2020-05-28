using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Utilities;
using SharpDX;
using SolarSystemSimulation.SolarSystem;
using Camera = HelixToolkit.Wpf.SharpDX.Camera;
using OrthographicCamera = HelixToolkit.Wpf.SharpDX.OrthographicCamera;
using PerspectiveCamera = HelixToolkit.Wpf.SharpDX.PerspectiveCamera;

namespace SolarSystemSimulation
{
    public partial class MainWindow
    {
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedMember.Local
        static NVOptimusEnabler nvEnabler = new NVOptimusEnabler();

        public EffectsManager EffectsManager { get; }
        public Camera Camera { get; }
        public Color4 LightColor { get; }
        public Stream EnvironmentMap { get; set; }

        private SolarSystem.SolarSystem _system;
        public ObservableValue<bool> CanStartSimulation { get; } = new ObservableValue<bool>();

        public int Planets { get; set; } = 5;
        public int SimulationTime { get; set; } = 60;
        public int TimeScale { get; set; } = 36;
        public bool IsDouble { get; set; }

        public ObservableCollection<AstronomicalObject> Bodies { get; set; } =
            new ObservableCollection<AstronomicalObject>();
        
        public MainWindow()
        {
            DataContext = this;

            EnvironmentMap = TextureHelper.LoadFileToMemory(@"Resources\skymap.dds");

            LightColor = new Color {R = 255, G = 255, B = 255, A = 155};
            EffectsManager = new DefaultEffectsManager();
            Camera = new PerspectiveCamera();

            InitializeComponent();

            Loaded += (sender, args) => InitCamera();
            Viewport3DX.SizeChanged += OnSizeChanged;

            DataContext = this;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            switch (Camera)
            {
                case OrthographicCamera camera:
                    camera.Width = e.NewSize.Width;
                    break;
            }
        }

        private void InitCamera()
        {
            Camera.LookAt(new Point3D(0, 0, 0),
                new Vector3D(-4e3, -4e3, -4e3),
                0);

            switch (Camera)
            {
                case PerspectiveCamera camera:
                    camera.FieldOfView = 60;
                    camera.FarPlaneDistance = AstronomicalObject.Au;
                    camera.NearPlaneDistance = 1;
                    break;
                case OrthographicCamera camera:
                    camera.FarPlaneDistance = AstronomicalObject.Au;
                    camera.NearPlaneDistance = 0;
                    break;
            }

            Viewport3DX.CameraMode = CameraMode.Inspect;
        }

        private void StartSimulation_OnClick(object sender, RoutedEventArgs e)
        {
            var but = (Button) sender;
            but.IsEnabled = false;

            Task.Run(() => _system.StartSimulation(480, TimeScale));
            Task.Delay(TimeSpan.FromSeconds(SimulationTime)).ContinueWith(_ =>
            {
                _system.IsRunning = false;
                but.Dispatcher?.Invoke(() => but.IsEnabled = true);
            });
        }

        private void GenerateSolarSystem_OnClick(object sender, RoutedEventArgs e)
        {
            RemoveAllBodies();

            _system = new SolarSystem.SolarSystem(IsDouble ? 2 : 1, Planets);
            _system.Bodies.ForEach(body =>
            {
                Viewport3DX.Items.Add(body);
                Bodies.Add(body);
            });
            _system.OrbitModels.ForEach(Viewport3DX.Items.Add);
            CanStartSimulation.Value = true;
        }

        private void LoadSolarSystem_OnClick(object sender, RoutedEventArgs e)
        {
            RemoveAllBodies();

            _system = new SolarSystem.SolarSystem(SolarSystem.SolarSystem.GetSolarSystem());
            _system.Bodies.ForEach(body =>
            {
                Viewport3DX.Items.Add(body);
                Bodies.Add(body);
            });
            _system.OrbitModels.ForEach(Viewport3DX.Items.Add);
            CanStartSimulation.Value = true;
        }

        private void RemoveAllBodies()
        {
            Bodies.Clear();
            for (var i = Viewport3DX.Items.Count - 1; i >= 0; i--)
            {
                var type = Viewport3DX.Items[i].GetType();
                if (type == typeof(AstronomicalObject) || type == typeof(LineGeometryModel3D))
                    Viewport3DX.Items.RemoveAt(i);
            }
        }
    }
}