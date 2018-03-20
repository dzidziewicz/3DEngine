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

        private DateTime _previousDate;
        private Device device;
        Mesh[] meshes;// = new Mesh("Cube", 8, 12);
        private ViewMatrixConfiguration _configuration;
        private ProjectionMatrixConfiguration _projectionMatrixConfiguration;
        Camera camera = new Camera();
        private DispatcherTimer _timer;
        private double _phi;
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
            _configuration = new ViewMatrixConfiguration();
            _projectionMatrixConfiguration = new ProjectionMatrixConfiguration(1, 100, 45, 1);
            device = new Device(Bmp, _phi, _configuration, _projectionMatrixConfiguration);

            // Our XAML Image control
            //Image.Source = Bmp;

            //mesh.Vertices[0] = new Point3D(-1, 1, 1);
            //mesh.Vertices[1] = new Point3D(1, 1, 1);
            //mesh.Vertices[2] = new Point3D(-1, -1, 1);
            //mesh.Vertices[3] = new Point3D(1, -1, 1);

            //mesh.Faces[0] = new Face(0, 1, 2);
            //mesh.Faces[1] = new Face(1, 2, 3);


            //mesh.Vertices[4] = new Point3D(-1, 1, -1);
            //mesh.Vertices[5] = new Point3D(1, 1, -1);
            //mesh.Vertices[6] = new Point3D(-1, -1, -1);
            //mesh.Vertices[7] = new Point3D(1, -1, -1);

            //mesh.Faces[2] = new Face(4, 5, 6);
            //mesh.Faces[3] = new Face(5, 6, 7);
            //mesh.Faces[4] = new Face(1, 3, 5);
            //mesh.Faces[5] = new Face(3, 5, 7);
            //mesh.Faces[6] = new Face(0, 2, 4);
            //mesh.Faces[7] = new Face(2, 4, 6);
            //mesh.Faces[8] = new Face(0, 1, 4);
            //mesh.Faces[9] = new Face(1, 4, 5);
            //mesh.Faces[10] = new Face(2, 6, 7);
            //mesh.Faces[11] = new Face(2, 3, 7);
            meshes = LoadMeshes.LoadJsonFileAsync("Suzanne.babylon");



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
            device = new Device(Bmp, _phi, _configuration, _projectionMatrixConfiguration);
            Bmp.Lock();
            device.Clear(0, 0, 0, 255);
            
            // Doing the various matrix operations
            device.Render(camera, meshes);
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
    }
}
