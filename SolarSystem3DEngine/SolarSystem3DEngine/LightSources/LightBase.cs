using System.Numerics;
using System.Windows.Media;

namespace SolarSystem3DEngine.LightSources
{
    public class LightBase
    {
        public Vector3 Position { get; set; }
        public Vector3 Intensity { get; set; }
        public Color Color { get; set; }

        public LightBase(Vector3 position, Color color)
        {
            Position = position;
            Color = color;
            Intensity = new Vector3(color.R, color.G, color.B);
        }
    }
}
