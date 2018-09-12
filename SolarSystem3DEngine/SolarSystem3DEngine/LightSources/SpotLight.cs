using System;
using System.Numerics;
using System.Windows.Media;
using MathNet.Numerics.LinearAlgebra.Double;

namespace SolarSystem3DEngine.LightSources
{
    public class SpotLight : LightBase
    {
        public double CosOfAngleOfAperture { get; set; } // should be in range 0 - 1
        public Point3D Direction { get; set; }          // point towards which spot light is directed
        public Point3D WorldDirection { get; set; }     // same as above but in world coordinates
        public double P { get; set; }                   // coeffitient describing light intensity distribution across cone

        public SpotLight(Point3D position, Color color, Point3D direction, double p, double cosOfAngleOfAperture = 0.2) : base(position, color)
        {
            Direction = direction;
            P = p;
            CosOfAngleOfAperture = cosOfAngleOfAperture;
        }

        public override Vector3 GetIntensityInPoint(Vector3 pointPosition)
        {
            var vectorToLight = Vector3.Normalize(WorldPosition - pointPosition);
            var reversedLightDirectionalVector = Vector3.Normalize(WorldPosition - WorldDirection);
            var cos = Vector3.Dot(reversedLightDirectionalVector, vectorToLight);

            if (cos < CosOfAngleOfAperture)
                return Vector3.Zero;    // given point is outside of the light cone

            cos = (float)Math.Pow(cos, P);
            return Vector3.Multiply(cos, Intensity);
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

            vectorCoordinates = DenseMatrix.OfArray(new[,]
            {
                {Direction.X},
                {Direction.Y},
                {Direction.Z},
                {Direction.W}
            });

            p = new Point3D(viewMatrix * vectorCoordinates);
            WorldDirection = p;
        }
    }
}
