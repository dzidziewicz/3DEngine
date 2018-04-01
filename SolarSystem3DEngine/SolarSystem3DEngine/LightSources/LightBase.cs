using System.Numerics;
using System.Windows.Media;

namespace SolarSystem3DEngine.LightSources
{
    public class LightBase
    {
        public Point3D Position { get; set; }
        public Point3D WorldPosition { get; set; }
        public Vector3 Intensity { get; set; }
        public Color Color { get; set; }

        public LightBase(Point3D position, Color color)
        {
            Position = position;
            Color = color;
            Intensity = new Vector3(color.R, color.G, color.B);
        }
    }
}
