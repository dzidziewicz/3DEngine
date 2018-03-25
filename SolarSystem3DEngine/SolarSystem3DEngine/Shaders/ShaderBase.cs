using System;
using System.Numerics;
using System.Windows.Media;
using SolarSystem3DEngine.Illuminations;

namespace SolarSystem3DEngine.Shaders
{
    public abstract class ShaderBase
    {
        //public Vertex v1 { get; set; }
        //public Vertex v2 { get; set; }
        //public Vertex v3 { get; set; }
        //protected Action<Point3D, Color> _drawPoint;

        //protected ShaderBase(Action<Point3D, Color> drawPoint)
        //{
        //    //_drawPoint = drawPoint;
        //}
        public BaseIllumination Illumination { get; set; }
        public Action<Point3D, Color> DrawPoint { get; set; }

        protected ShaderBase(BaseIllumination illumination)
        {
            Illumination = illumination;
        }
        public abstract void DrawTriangle(Vertex v1, Vertex v2, Vertex v3);


        protected Color InterpolateColor(Color c1, Color c2, double gradient)
        {
            var r = (byte)Math.Round(Computations.Interpolate(c1.R, c2.R, gradient));
            var g = (byte)Math.Round(Computations.Interpolate(c1.G, c2.G, gradient));
            var b = (byte)Math.Round(Computations.Interpolate(c1.B, c2.B, gradient));
            return Color.FromArgb(1, r, g, b);
        }

        protected Vector3 InterpolateVector(Vector3 v1, Vector3 v2, double gradient)
        {
            var x = (float)Computations.Interpolate(v1.X, v2.X, gradient);
            var y = (float)Computations.Interpolate(v1.Y, v2.Y, gradient);
            var z = (float)Computations.Interpolate(v1.Z, v2.Z, gradient);
            return new Vector3(x, y, z);
        }
    }
}
