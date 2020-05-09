using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Utilities;
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

        private SolarSystem.SolarSystem _system;

        public MainWindow()
        {
            _system = new SolarSystem.SolarSystem();

            LightColor = new Color {ScR = 1, ScG = 1, ScB = 1, ScA = 0.1f};
            EffectsManager = new DefaultEffectsManager();
            Camera = new OrthographicCamera();

            var device = EffectsManager.Device;

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