using System.Numerics;

namespace SolarSystem3DEngine
{
    public class Camera
    {
        public Vector3 Position { get; set; }
        public Vector3 Target { get; set; }

        public Camera(Vector3 position, Vector3 target)
        {
            Position = position;
            Target = target;
        }
    }
}
