using System;
using System.Numerics;
using System.Windows.Media;

namespace SolarSystem3DEngine
{
    public static class Computations
    {
        // Clamping values to keep them between 0 and 1
        public static double Clamp(double value, double min = 0, double max = 1)
        {
            return Math.Max(min, Math.Min(value, max));
        }

        // Interpolating the value between 2 vertices 
        // min is the starting point, max the ending point
        // and gradient the % between the 2 points
        public static double Interpolate(double min, double max, double gradient)
        {
            return min + (max - min) * Clamp(gradient);
        }

        // Compute the cosine of the angle between the light vector and the normal vector
        // Returns a value between 0 and 1
        public static float ComputeNDotL(Vector3 vertex, Vector3 normal, Vector3 lightPosition)
        {
            var lightDirection = lightPosition - vertex;

            normal = Vector3.Normalize(normal);
            lightDirection = Vector3.Normalize(lightDirection);

            return Math.Max(0, Vector3.Dot(normal, lightDirection));
        }

        public static Point3D Scale(Point3D point, int renderWidth, int renderHeight)
        {
            return new Point3D(ScaleW(point.X, renderWidth), ScaleH(point.Y, renderHeight), point.Z, point.W);
        }

        public static double ScaleW(double p, int renderWidth)
        {
            return (p + 1) / 2 * renderWidth; //point.X * _bmp.PixelWidth + _bmp.PixelWidth / 2.0f;
        }

        public static double ScaleH(double p, int renderHeight)
        {
            return (p + 1) / 2 * renderHeight; //-point.Y * _bmp.PixelHeight + _bmp.PixelHeight / 2.0f;
        }

        public static int ConvertToArgb32(Color color)
        {
            // BGRA is used by Windows instead by RGBA in HTML5
            return ((color.R << 16) | (color.G << 8) | (color.B << 0) | (0xFF << 24));
        }

        public static float NormalizeCosinus(float cos)
        {
            return Math.Max(0, Math.Min(1, cos));
        }
    }
}
