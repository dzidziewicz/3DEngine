using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using MathNet.Numerics.Optimization;
using SolarSystem3DEngine.LightSources;

namespace SolarSystem3DEngine.Illuminations
{
    public abstract class BaseIllumination
    {
        public Vector3 KAmbient { get; set; }
        public Vector3 KDiffuse { get; set; }
        public Vector3 KSpecular { get; set; }

        public List<LightBase> Lights { get; set; }

        protected BaseIllumination(IEnumerable<LightBase> lights)
        {
            Lights = lights.ToList();
        }

        public Color GetPixelColor(Vector3 normal, Vector3 viewerPosition, Vector3 position)
        {
            var intensity = GetAmbient();
            normal = Vector3.Normalize(normal);
            var vectorToViewer = Vector3.Normalize(viewerPosition - position);
            foreach (var light in Lights)
            {
                var vectorToLight = Vector3.Normalize(light.WorldPosition - position);
                var diffPlusSpec = GetDiffuse(vectorToLight, normal) +
                                   GetSpecular(vectorToLight, vectorToViewer, normal, 1);
                intensity += Vector3.Multiply(light.Intensity, diffPlusSpec);
            }

            return VectorToColor(intensity);
        }

        protected abstract Vector3 GetSpecular(Vector3 vectorToLight, Vector3 vectorToViewer, Vector3 normal, int m);

        protected Vector3 GetAmbient()
        {
            return KAmbient;
        }

        protected Vector3 GetDiffuse(Vector3 light, Vector3 normal)
        {
            //var cos = Vector3.Dot(normal, light);
            var cos = Computations.NormalizeCosinus(Vector3.Dot(normal, light));// Math.Max(0, Math.Min(1, Vector3.Dot(normal, light)));
            return Vector3.Multiply(cos, KDiffuse);

        }

        protected Color VectorToColor(Vector3 I)
        {
            var r = (byte)(int)Math.Min(I.X, 255);
            var g = (byte)(int)Math.Min(I.Y, 255);
            var b = (byte)(int)Math.Min(I.Z, 255);
            return Color.FromArgb(1, r, g, b);
        }

        public void  SetCoeffitients(Vector3 kAmbient, Vector3 kDiffuse, Vector3 kSpecular)
        {
            KAmbient = kAmbient;
            KDiffuse = kDiffuse;
            KSpecular = kSpecular;
        }
    }
}
