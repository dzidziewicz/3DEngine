using System;
using System.Numerics;
using System.Windows.Media;
using SolarSystem3DEngine.Illuminations;

namespace SolarSystem3DEngine.Shaders
{
    public abstract class ShaderBase
    {
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

        protected void SortVertices(ref Vertex v1, ref Vertex v2, ref Vertex v3)
        {
            // Sorting the points in order to always have this order on screen p1, p2 & p3
            // with p1 always up (thus having the Y the lowest possible to be near the top screen)
            // then p2 between p1 & p3

            if (v1.Coordinates.Y > v2.Coordinates.Y)
            {
                var temp = v2;
                v2 = v1;
                v1 = temp;
            }

            if (v2.Coordinates.Y > v3.Coordinates.Y)
            {
                var temp = v2;
                v2 = v3;
                v3 = temp;
            }

            if (v1.Coordinates.Y > v2.Coordinates.Y)
            {
                var temp = v2;
                v2 = v1;
                v1 = temp;
            }
        }
    }
}
