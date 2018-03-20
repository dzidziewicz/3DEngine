using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;

namespace SolarSystem3DEngine
{
    public class ProjectionMatrixConfiguration
    {
        public int N { get; set; }
        public double F { get; set; }
        public double Fov { get; set; }
        public double A { get; set; }
        public DenseMatrix ProjectionMatrix { get; set; }

        public ProjectionMatrixConfiguration(int n, double f, double fov, double a)
        {
            N = n;
            F = f;
            Fov = fov;
            A = a;
            ProjectionMatrix = CalculateProjectionMatrix();
        }

        private DenseMatrix CalculateProjectionMatrix()
        {
            return DenseMatrix.OfArray(new [,]
            {
                { Math.E, 0, 0, 0 },
                { 0, Math.E / A, 0, 0 },
                { 0, 0, -(F + N)/(F - N), -2*F*N/(F - N) },
                { 0, 0, -1, 0 }
            });
        }
    }
}
