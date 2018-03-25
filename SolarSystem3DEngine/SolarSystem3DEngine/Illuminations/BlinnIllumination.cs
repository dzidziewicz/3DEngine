using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using SolarSystem3DEngine.LightSources;

namespace SolarSystem3DEngine.Illuminations
{
    public class BlinnIllumination: BaseIllumination
    {
        public BlinnIllumination(IEnumerable<LightBase> lights) : base(lights)
        {
        }

        protected override Vector3 GetSpecular(Vector3 vectorToLight, Vector3 vectorToViewer, Vector3 normal, int m)
        {
            var H = (vectorToLight + vectorToViewer) / 2;
            var cos = Computations.NormalizeCosinus(Vector3.Dot(normal, H));
            cos = (float)Math.Pow(cos, m);
            return KSpecular * cos;
        }


    }
}
