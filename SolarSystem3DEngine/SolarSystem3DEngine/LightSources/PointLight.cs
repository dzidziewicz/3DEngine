using System.Numerics;
using System.Windows.Media;
using MathNet.Numerics.LinearAlgebra.Double;

namespace SolarSystem3DEngine.LightSources
{
    public class PointLight : LightBase
    {
        public PointLight(Point3D position, Color color) : base(position, color)
        {
        }

        public override Vector3 GetIntensityInPoint(Vector3 pointPosition)
        {
            return Intensity; // point light shines in every direction the same
        }

        public override void UpdateWorldCoordinates(DenseMatrix viewMatrix)
        {
            var vectorCoordinates = DenseMatrix.OfArray(new[,]
            {
                {Position.X},
                {Position.Y},
                {Position.Z},
                {Position.W}
            });

            var p = new Point3D(viewMatrix * vectorCoordinates);
            WorldPosition = p;
        }
    }
}
