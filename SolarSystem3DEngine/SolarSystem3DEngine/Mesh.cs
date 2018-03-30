using System.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;

namespace SolarSystem3DEngine
{
    public class Mesh
    {
        public string Name { get; set; }
        public Vertex[] Vertices { get; private set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public Face[] Faces { get; set; }

        public Vector3 KAmbient { get; set; }
        public Vector3 KDiffuse { get; set; }
        public Vector3 KSpecular { get; set; }

        public DenseMatrix ViewModelMatrix { get; set; }
        public DenseMatrix NormalMatrix { get; set; }

        public Mesh(string name, int verticesCount, int facesCount)
        {
            Vertices = new Vertex[verticesCount];
            Faces = new Face[facesCount];
            Name = name;
        }

        public void SetCoeffitients(Vector3 kAmbient, Vector3 kDiffuse, Vector3 kSpecular)
        {
            KAmbient = kAmbient;
            KDiffuse = kDiffuse;
            KSpecular = kSpecular;
        }
    }
}
