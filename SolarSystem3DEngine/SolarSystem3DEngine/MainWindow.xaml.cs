using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using SolarSystem3DEngine.Illuminations;
using SolarSystem3DEngine.LightSources;
using SolarSystem3DEngine.Shaders;
using SolarSystem3DEngine.SoftEngine;

namespace SolarSystem3DEngine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public WriteableBitmap Bmp { get; set; }
        public string Fps { get; set; }
        public bool PhongIlluminationChecked { get; set; }
        public bool GoraudShadingChecked { get; set; }

        private DateTime _previousDate;
        private Device device;
        private Mesh[] _meshes;
        private ViewMatrixConfiguration _configuration;
        private ProjectionMatrixConfiguration _projectionMatrixConfiguration;
        Camera camera = new Camera();
        private DispatcherTimer _timer;
        private double _phi;
        private PointLight[] _pointLights;
        private GoraudShader _goraudShaderWithPhong;
        private GoraudShader _goraudShaderWithBlinn;
        private PhongShader _phongShaderWithPhong;
        private PhongShader _phongShaderWithBlinn;
        private ShaderBase _currentShader;
        private PhongIllumination _phong;
        private BlinnIllumination _blinn;

        public MainWindow()
        {
            InitializeComponent();
            Page_Loaded();
            _phi = 0;
            DataContext = this;
        }

        private void Page_Loaded()
        {
            // Choose the back buffer resolution here
            Bmp = BitmapFactory.New((int)Image.Width, (int)Image.Height);
            Bmp.Clear(Colors.Red);
            PhongIlluminationChecked = GoraudShadingChecked = true;
            _configuration = new ViewMatrixConfiguration();
            _projectionMatrixConfiguration = new ProjectionMatrixConfiguration(1, 100, 45, 1);
            _pointLights = new [] {new PointLight(new Vector3(3, 240, 10), Colors.White)};
            _phong = new PhongIllumination(_pointLights);
            _blinn = new BlinnIllumination(_pointLights);
            _currentShader = _goraudShaderWithPhong = new GoraudShader(_phong);
            _goraudShaderWithBlinn = new GoraudShader(_blinn);
            _phongShaderWithPhong = new PhongShader(_phong);
            _phongShaderWithBlinn = new PhongShader(_blinn);
            //device = new Device(Bmp, _phi, _configuration, _projectionMatrixConfiguration, _pointLights, _goraudShaderWithPhong);

            //_meshes = LoadMeshes.LoadJsonFileAsync("Suzanne.babylon");
            _meshes = LoadMeshes.LoadJsonFileAsync("sphere.babylon");
            _meshes[0].SetCoeffitients(new Vector3(0, 0 ,0), new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.5f, 0.5f, 0.5f)); //0.5f, 0.5f, 0.5f


            camera.Position = new Vector3(3.5f, 0.5f, 0.5f);
            camera.Target = new Vector3(0, 0.1f, 0.5f);

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1 / 60.0)
            };
            _timer.Tick += CompositionTarget_Rendering;
            //_timer.Start();
            // Registering to the XAML rendering loop
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        // Rendering loop handler
        private void CompositionTarget_Rendering(object sender, object e)
        {
            _phi += 0.01;
            var now = DateTime.Now;
            var currentFps = 1000.0 / (now - _previousDate).TotalMilliseconds;
            _previousDate = now;

            Fps = $"{currentFps:0.00} fps";
            OnPropertyChanged("Fps");
            device = new Device(Bmp, _phi, _configuration, _projectionMatrixConfiguration, _pointLights, _currentShader);
            Bmp.Lock();
            device.Clear(0, 0, 0, 255);
            
            // Doing the various matrix operations
            device.Render(camera, _meshes);
            // Flushing the back buffer into the front buffer
            device.Present();
            Bmp.Unlock();
        }

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.VerifyPropertyName(propertyName);

            if (this.PropertyChanged != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                this.PropertyChanged(this, e);
                //_repaint();
            }
        }

        #endregion // INotifyPropertyChanged Members

        #region Debugging Aides

        /// <summary>
        /// Warns the developer if this object does not have
        /// a public property with the specified name. This
        /// method does not exist in a Release build.
        /// </summary>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public virtual void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real,
            // public, instance property on this object.
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                string msg = "Invalid property name: " + propertyName;

                if (this.ThrowOnInvalidPropertyName)
                    throw new Exception(msg);
                else
                    Debug.Fail(msg);
            }
        }

        /// <summary>
        /// Returns whether an exception is thrown, or if a Debug.Fail() is used
        /// when an invalid property name is passed to the VerifyPropertyName method.
        /// The default value is false, but subclasses used by unit tests might
        /// override this property's getter to return true.
        /// </summary>
        protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

        #endregion // Debugging Aides

        private void PhongIlluminationChcecked(object sender, RoutedEventArgs e)
        {
            _currentShader = GoraudShadingChecked ? (ShaderBase) _goraudShaderWithPhong : _phongShaderWithPhong;
        }

        private void BlinnIlluminationChecked(object sender, RoutedEventArgs e)
        {
            _currentShader = GoraudShadingChecked ? (ShaderBase) _goraudShaderWithBlinn : _phongShaderWithBlinn;
        }

        private void GoraudShaderChecked(object sender, RoutedEventArgs e)
        {
            _currentShader = PhongIlluminationChecked ? _goraudShaderWithPhong : _goraudShaderWithBlinn;
        }

        private void PhongShaderChecked(object sender, RoutedEventArgs e)
        {
            _currentShader = PhongIlluminationChecked ? _phongShaderWithPhong : _phongShaderWithBlinn;
        }
    }
}
