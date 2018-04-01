using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
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

        private double _phi;
        private DateTime _previousDate;
        private Device _device;
        private List<Mesh> _meshes;
        private ViewMatrixConfiguration _configuration;
        private ProjectionMatrixConfiguration _projectionMatrixConfiguration;
        private DenseMatrix _projectionViewMatrix;
        private Camera _stationaryCamera;
        private Camera _currentCamera;
        private Camera _followingEarthCamera;
        private PointLight[] _pointLights;
        private GoraudShader _goraudShaderWithPhong;

        #region Constants

        private readonly Vector3 _cameraPosition = new Vector3(11f, 1f, -15);
        private readonly Vector3 _stationaryCameraTarget = new Vector3(1, 1, 0);
        private readonly Func<double, double> _earthModelCoordinateOriginX = (phi) => 1 + 2 * Math.Sin(phi);
        private readonly Func<double, double> _earthModelCoordinateOriginY = (phi) => 1 + 2 * Math.Cos(phi);
        private readonly Point3D _lightPosition = new Point3D(1, 1, -1);
        #endregion
        private void Page_Loaded()
        {
            // Choose the back buffer resolution here
            Bmp = BitmapFactory.New((int)Image.Width, (int)Image.Height);
            _meshes = new List<Mesh>();
//            var mesh = LoadMeshes.LoadJsonFileAsync("Suzanne.babylon");
            //            var mesh = LoadMeshes.LoadJsonFileAsync("plane.babylon");
                        var mesh = LoadMeshes.LoadJsonFileAsync("sphere.babylon");
            _meshes.Add(mesh);
            _meshes[0].SetCoeffitients(new Vector3(0.2f, 0.2f, 0.2f), new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.5f, 0.5f, 0.5f)); //0.5f, 0.5f, 0.5f

            PhongIlluminationChecked = GoraudShadingChecked = true;
            _currentCamera = _stationaryCamera = new Camera(_cameraPosition, _stationaryCameraTarget);
            _followingEarthCamera = new Camera(_stationaryCamera.Position,
                new Vector3((float)_earthModelCoordinateOriginX(_phi), (float)_earthModelCoordinateOriginY(_phi), 0));


            _projectionMatrixConfiguration = new ProjectionMatrixConfiguration(1, 100, 45, 1);
            _pointLights = new[] { new PointLight(_lightPosition, Colors.White) /*, new PointLight(new Vector3(0, 240, 10), Colors.Red)*/ };
            UpdateMatricesConfigurations();
            _phong = new PhongIllumination(_pointLights);
            _blinn = new BlinnIllumination(_pointLights);
            _currentShader = _goraudShaderWithPhong = new GoraudShader(_phong);
            _goraudShaderWithBlinn = new GoraudShader(_blinn);
            _phongShaderWithPhong = new PhongShader(_phong);
            _phongShaderWithBlinn = new PhongShader(_blinn);

            UpdateDevice();

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        #region Shaders and Illuminations

        private GoraudShader _goraudShaderWithBlinn;

        private PhongShader _phongShaderWithPhong;

        private PhongShader _phongShaderWithBlinn;

        private ShaderBase _currentShader;

        private PhongIllumination _phong;

        private BlinnIllumination _blinn;

        #endregion


        public MainWindow()
        {
            InitializeComponent();
            Page_Loaded();
            _phi = 0;
            DataContext = this;
        }

        // Rendering loop handler
        private void CompositionTarget_Rendering(object sender, object e)
        {
            _phi += 0.01;
            if (_currentCamera.Equals(_followingEarthCamera))
                UpdateFollowingEarthCamera();
            UpdateEarthModelMatrix();

            UpdateFps();

            Rerender();
        }

        private void Rerender()
        {
            Bmp.Lock();

            _device.Clear(0, 0, 0, 255);
            _device.Render(_currentCamera, _meshes);

            var p = new Point3D(1f, 1f, -1f);
            var vectorCoordinates = DenseMatrix.OfArray(new[,]
            {
                {p.X},
                {p.Y},
                {p.Z},
                {p.W}
            });

            var pprim = _projectionViewMatrix * vectorCoordinates;
            var w = pprim[3, 0];
            var newCoordinates = new Point3D(pprim) / w;
            newCoordinates = Computations.Scale(newCoordinates, Bmp.PixelWidth, Bmp.PixelHeight);
            Bmp.DrawEllipseCentered((int)newCoordinates.X, (int)newCoordinates.Y, 1, 1, Colors.Red);

            _device.Present();
            Bmp.Unlock();
        }

        private void UpdateEarthModelMatrix()
        {
            var earthModelMatrix = DenseMatrix.OfArray(new double[,]
            {
                {Math.Cos(_phi), -Math.Sin(_phi), 0, _earthModelCoordinateOriginX(_phi)},
                {Math.Sin(_phi), Math.Cos(_phi), 0, _earthModelCoordinateOriginY(_phi)},
                {0, 0, 1, 0},
                {0, 0, 0, 1}
            });
            var viewModelMatrix = _configuration.ViewMatrix * earthModelMatrix;
            _meshes[0].ViewModelMatrix = viewModelMatrix;

            _meshes[0].ProjectionViewModelMatrix = _projectionMatrixConfiguration.ProjectionMatrix * viewModelMatrix;

            var invertedNormalMatrix = Matrix<double>.Build.DenseOfColumnMajor(4, 4, viewModelMatrix.Values);
            _meshes[0].NormalMatrix = DenseMatrix.OfMatrix(invertedNormalMatrix.Inverse().Transpose());
        }

        private void UpdateFollowingEarthCamera()
        {

            _followingEarthCamera.Target = new Vector3((float)_earthModelCoordinateOriginX(_phi),
                (float)_earthModelCoordinateOriginY(_phi), 0);
            UpdateMatricesConfigurations();
        }

        private void UpdateMatricesConfigurations()
        {
            _configuration = new ViewMatrixConfiguration(_currentCamera.Position, _currentCamera.Target, new Vector3(0, 0, 1));
            _projectionViewMatrix = _projectionMatrixConfiguration.ProjectionMatrix * _configuration.ViewMatrix;
            foreach (var light in _pointLights)
            {
                var vectorCoordinates = DenseMatrix.OfArray(new[,]
                {
                    {light.Position.X},
                    {light.Position.Y},
                    {light.Position.Z},
                    {light.Position.W}
                });

                var p = new Point3D(_configuration.ViewMatrix * vectorCoordinates);
                light.WorldPosition = p;
            }
            //UpdateEarthModelMatrix();
        }

        private void UpdateFps()
        {
            var now = DateTime.Now;
            var currentFps = 1000.0 / (now - _previousDate).TotalMilliseconds;
            _previousDate = now;

            Fps = $"{currentFps:0.00} fps";
            OnPropertyChanged("Fps");
        }

        private void UpdateDevice()
        {
            _device = new Device(Bmp, _pointLights, _currentShader);
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
            _currentShader = GoraudShadingChecked ? (ShaderBase)_goraudShaderWithPhong : _phongShaderWithPhong;
            UpdateDevice();
        }

        private void BlinnIlluminationChecked(object sender, RoutedEventArgs e)
        {
            _currentShader = GoraudShadingChecked ? (ShaderBase)_goraudShaderWithBlinn : _phongShaderWithBlinn;
            UpdateDevice();
        }

        private void GoraudShaderChecked(object sender, RoutedEventArgs e)
        {
            _currentShader = PhongIlluminationChecked ? _goraudShaderWithPhong : _goraudShaderWithBlinn;
            UpdateDevice();
        }

        private void PhongShaderChecked(object sender, RoutedEventArgs e)
        {
            _currentShader = PhongIlluminationChecked ? _phongShaderWithPhong : _phongShaderWithBlinn;
            UpdateDevice();
        }

        private void StationaryCameraChecked(object sender, RoutedEventArgs e)
        {
            _currentCamera = _stationaryCamera;
            UpdateMatricesConfigurations();
        }

        private void FollowingEarthCameraChecked(object sender, RoutedEventArgs e)
        {
            _currentCamera = _followingEarthCamera;
        }
    }
}
