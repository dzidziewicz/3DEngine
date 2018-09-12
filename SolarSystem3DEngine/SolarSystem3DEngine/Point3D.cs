using System;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;

namespace SolarSystem3DEngine
{
    public class Point3D
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double W { get; set; }

        public Point3D(double x, double y, double z, double w = 1)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public Point3D(DenseMatrix vector)
        {
            if(vector.ColumnCount != 1 || vector.RowCount < 3 || vector.RowCount > 4 )
                throw new ArgumentException("Incorrect vector count in conversion to Point3D");
            X = vector[0, 0];
            Y = vector[1, 0];
            Z = vector[2, 0];
            W = (vector.RowCount == 4) ? vector[3, 0] : 1;
        }

        public Point3D(Vector3 v)
        {
            X = v.X;
            Y = v.Y;
            Z = v.Z;
            W = 0;
        }

        public static implicit operator Vector2(Point3D point)
        {
            return new Vector2((float)point.X, (float)point.Y);
        }

        public static implicit operator Vector3(Point3D point)
        {
            return new Vector3((float)point.X, (float)point.Y, (float)point.Z);
        }

        public static Point3D operator +(Point3D p1, Point3D p2)
        {
            return new Point3D(p1.X + p2.X, p1.Y + p2.Y, p1.Z + p2.Z, p1.W + p2.W);
        }

        public static Point3D operator -(Point3D p1, Point3D p2)
        {
            return new Point3D(p1.X - p2.X, p1.Y - p2.Y, p1.Z - p2.Z, p1.W - p2.W);
        }
        public static Point3D operator /(Point3D p, double n)
        {
            return new Point3D(p.X / n, p.Y / n, p.Z / n, p.W / n);
        }
    }
}
