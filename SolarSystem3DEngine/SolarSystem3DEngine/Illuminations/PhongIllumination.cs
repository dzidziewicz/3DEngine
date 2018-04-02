using System;
using System.Collections.Generic;
using System.Numerics;
using SolarSystem3DEngine.LightSources;

namespace SolarSystem3DEngine.Illuminations
{
    public class PhongIllumination: BaseIllumination
    {
        public PhongIllumination(IEnumerable<LightBase> lights) : base(lights)
        {
        }

        protected override Vector3 GetSpecular(Vector3 vectorToLight, Vector3 vectorToViewer, Vector3 normal, int m)
        {
            var R = 2 * Vector3.Multiply(Vector3.Dot(normal, vectorToLight), normal) - vectorToLight;
            var cos = Computations.NormalizeCosinus(Vector3.Dot(vectorToViewer, R));
            cos = (float) Math.Pow(cos, m);
            return KSpecular * cos;
        }
    }
}
