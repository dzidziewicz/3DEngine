using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra;

namespace SolarSystem3DEngine
{
    public class ViewMatrixConfiguration
    {
        public Vector3 CameraPosition { get; set; }
        public Vector3 CameraTarget { get; set; }
        public Vector3 UpVector { get; set; }
        public DenseMatrix ViewMatrix { get; set; }

        public ViewMatrixConfiguration(Vector3 cameraPosition, Vector3 cameraTarget, Vector3 upVector)
        {
            CameraPosition = cameraPosition;
            CameraTarget = cameraTarget;
            UpVector = upVector;
            ViewMatrix = CalculateViewMatrix();
        }

        public ViewMatrixConfiguration()
        {
            CameraPosition = new Vector3((float)3.5, (float)0.5, (float)-5);
            CameraTarget = new Vector3(0, 0.1f, 0.5f);
            UpVector = new Vector3(0, 0, 1f);
            ViewMatrix = CalculateViewMatrix();
        }

        private DenseMatrix CalculateViewMatrix()
        {
            var zAxis = CameraPosition - CameraTarget;
            zAxis = Vector3.Normalize(zAxis);

            var xAxis = MultiplyVectors(UpVector, zAxis);
            xAxis = Vector3.Normalize(xAxis);

            var yAxis = MultiplyVectors(zAxis, xAxis);
            yAxis = Vector3.Normalize(yAxis);

            var invertedViewMatrix = Matrix<double>.Build.DenseOfRowArrays(new double[] { xAxis.X, yAxis.X, zAxis.X, CameraPosition.X },
                                                                             new double[] { xAxis.Y, yAxis.Y, zAxis.Y, CameraPosition.Y },
                                                                            new double[] { xAxis.Z, yAxis.Z, zAxis.Z, CameraPosition.Z },
                                                                            new double[] { 0, 0, 0, 1 });

            var viewMatrix4X4 = invertedViewMatrix.Inverse();
            var viewMatrix = DenseMatrix.OfMatrix(viewMatrix4X4);
            return viewMatrix;
        }

        private Vector3 MultiplyVectors(Vector3 a, Vector3 b)
        {
            return new Vector3(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
        }
    }
}
