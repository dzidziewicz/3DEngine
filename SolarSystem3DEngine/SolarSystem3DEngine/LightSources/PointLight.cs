using System.Numerics;
using System.Windows.Media;

namespace SolarSystem3DEngine.LightSources
{
    public class PointLight : LightBase
    {
        public PointLight(Point3D position, Color color) : base(position, color)
        {
        }
    }
}
