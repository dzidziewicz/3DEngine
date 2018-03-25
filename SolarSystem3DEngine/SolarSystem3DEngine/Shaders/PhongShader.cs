using System.Numerics;
using SolarSystem3DEngine.Illuminations;

namespace SolarSystem3DEngine.Shaders
{
    public class PhongShader: ShaderBase
    {
        public PhongShader(BaseIllumination illumination) : base(illumination)
        {
        }

        public override void DrawTriangle(Vertex v1, Vertex v2, Vertex v3)
        {
            // Sorting the points in order to always have this order on screen p1, p2 & p3
            // with p1 always up (thus having the Y the lowest possible to be near the top screen)
            // then p2 between p1 & p3
            if (v1.Coordinates.Y > v2.Coordinates.Y)
            {
                var temp = v2;
                v2 = v1;
                v1 = temp;
            }

            if (v2.Coordinates.Y > v3.Coordinates.Y)
            {
                var temp = v2;
                v2 = v3;
                v3 = temp;
            }

            if (v1.Coordinates.Y > v2.Coordinates.Y)
            {
                var temp = v2;
                v2 = v1;
                v1 = temp;
            }

            var p1 = v1.Coordinates;
            var p2 = v2.Coordinates;
            var p3 = v3.Coordinates;

            var viewerPosition = new Vector3(320, 240, 0);

            var data = new PhongScanlineData();

            // inverse slopes
            var dP1P2 = (p2.Y - p1.Y > 0) ? (p2.X - p1.X) / (p2.Y - p1.Y) : 0;

            var dP1P3 = (p3.Y - p1.Y > 0) ? (p3.X - p1.X) / (p3.Y - p1.Y) : 0;

            // First case where P2 is on the right of P1P3
            if (dP1P2 > dP1P3)
            {
                for (var y = (int)p1.Y; y < (int)p2.Y; y++)
                {
                    data.CurrentY = y;
                    data.VertexA = v1;
                    data.VertexB = v3;
                    data.VertexC = v1;
                    data.VertexD = v2;
                    ProcessScanLine(data, v1, v3, v1, v2, viewerPosition);
                }

                for (var y = (int)p2.Y; y <= (int)p3.Y; y++)
                {
                    data.CurrentY = y;
                    data.VertexA = v1;
                    data.VertexB = v3;
                    data.VertexC = v2;
                    data.VertexD = v3;
                    ProcessScanLine(data, v1, v3, v2, v3, viewerPosition);
                }
            }
            // First case where P2 is on the left of P1P3
            else
            {
                for (var y = (int)p1.Y; y < (int)p2.Y; y++)
                {
                    data.CurrentY = y;
                    data.VertexA = v1;
                    data.VertexB = v2;
                    data.VertexC = v1;
                    data.VertexD = v3;
                    ProcessScanLine(data, v1, v2, v1, v3, viewerPosition);
                }

                for (var y = (int)p2.Y; y <= (int)p3.Y; y++)
                {
                    data.CurrentY = y;
                    data.VertexA = v2;
                    data.VertexB = v3;
                    data.VertexC = v1;
                    data.VertexD = v3;
                    ProcessScanLine(data, v2, v3, v1, v3, viewerPosition);
                }
            }
        }

        // drawing line between 2 points from left to right
        // papb -> pcpd
        // pa, pb, pc, pd must then be sorted before
        private void ProcessScanLine(PhongScanlineData data, Vertex va, Vertex vb, Vertex vc, Vertex vd, Vector3 viewerPosition)
        {
            var pa = va.Coordinates;
            var pb = vb.Coordinates;
            var pc = vc.Coordinates;
            var pd = vd.Coordinates;

            // Thanks to current Y, we can compute the gradient to compute others values like
            // the starting X (sx) and ending X (ex) to draw between
            // if pa.Y == pb.Y or pc.Y == pd.Y, gradient is forced to 1
            var gradient1 = pa.Y != pb.Y ? (data.CurrentY - pa.Y) / (pb.Y - pa.Y) : 1;
            var gradient2 = pc.Y != pd.Y ? (data.CurrentY - pc.Y) / (pd.Y - pc.Y) : 1;

            var startNormalVector = InterpolateVector(data.VertexA.Normal, data.VertexB.Normal, gradient1);
            var endNormalVector = InterpolateVector(data.VertexC.Normal, data.VertexD.Normal, gradient2);
            var startCoordinates =
                InterpolateVector(data.VertexA.Coordinates, data.VertexB.Coordinates, gradient1);
            var endCoordinates =
                InterpolateVector(data.VertexC.Coordinates, data.VertexD.Coordinates, gradient2);
            var startWorldCoordinates =
                InterpolateVector(data.VertexA.WorldCoordinates, data.VertexB.WorldCoordinates, gradient1);
            var endWorldCoordinates =
                InterpolateVector(data.VertexC.WorldCoordinates, data.VertexD.WorldCoordinates, gradient2);

            var endX = (int)endCoordinates.X;
            var startX = (int) startCoordinates.X;
            var gradient = 0d;
            var step = 1d / (endX - startX);
            for (var x = startX; x <= endX; x++, gradient += step)
            {
                var pixelCoordinates = InterpolateVector(startCoordinates, endCoordinates, gradient);
                var pixelNormal = InterpolateVector(startNormalVector, endNormalVector, gradient);
                var pixelWorldCoordinates = InterpolateVector(startWorldCoordinates, endWorldCoordinates, gradient);
                var color = Illumination.GetPixelColor(pixelNormal, viewerPosition, pixelWorldCoordinates);

                DrawPoint(new Point3D(pixelCoordinates), color);
            }
        }
    }
}
