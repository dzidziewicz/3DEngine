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
            SortVertices(ref v1, ref v2, ref v3);

            var p1 = v1.Coordinates;
            var p2 = v2.Coordinates;
            var p3 = v3.Coordinates;

            var viewerPosition = Vector3.Zero;


            // inverse slopes
            var dP1P2 = (p2.Y - p1.Y > 0) ? (p2.X - p1.X) / (p2.Y - p1.Y) : 0;

            var dP1P3 = (p3.Y - p1.Y > 0) ? (p3.X - p1.X) / (p3.Y - p1.Y) : 0;

            // First case where P2 is on the right of P1P3
            if (dP1P2 > dP1P3)
            {
                var endY = (int)(p2.Y + 0.5);

                for (var y = (int)p1.Y; y < endY; y++)
                    ProcessScanLine(y, v1, v3, v1, v2, viewerPosition);

                endY = (int)(p3.Y+0.5);
                for (var y = (int)p2.Y; y <= endY; y++)
                    ProcessScanLine(y, v1, v3, v2, v3, viewerPosition);
            }
            // First case where P2 is on the left of P1P3
            else
            {
                var endY = (int)(p2.Y+0.5);
                for (var y = (int)p1.Y; y < endY; y++)
                    ProcessScanLine(y, v1, v2, v1, v3, viewerPosition);

                endY = (int) (p3.Y + 0.5);
                for (var y = (int)p2.Y; y <= endY; y++)
                    ProcessScanLine(y, v2, v3, v1, v3, viewerPosition);
            }
        }

        // drawing line between 2 points from left to right
        // papb -> pcpd
        // pa, pb, pc, pd must then be sorted before
        private void ProcessScanLine(int currentY, Vertex va, Vertex vb, Vertex vc, Vertex vd, Vector3 viewerPosition)
        {
            var pa = va.Coordinates;
            var pb = vb.Coordinates;
            var pc = vc.Coordinates;
            var pd = vd.Coordinates;

            // Thanks to current Y, we can compute the gradient to compute others values like
            // the starting X (sx) and ending X (ex) to draw between
            // if pa.Y == pb.Y or pc.Y == pd.Y, gradient is forced to 1

            var gradient1 = pa.Y != pb.Y ? (currentY - pa.Y) / (pb.Y - pa.Y) : 1;
            var gradient2 = pc.Y != pd.Y ? (currentY - pc.Y) / (pd.Y - pc.Y) : 1;

            var startNormalVector = InterpolateVector(va.Normal, vb.Normal, gradient1);
            var endNormalVector = InterpolateVector(vc.Normal, vd.Normal, gradient2);

            // interpolated pixel screen coordinates
            var startCoordinates =
                InterpolateVector(va.Coordinates, vb.Coordinates, gradient1);
            var endCoordinates =
                InterpolateVector(vc.Coordinates, vd.Coordinates, gradient2);

            // interpolated objects' coordinates in 3D world 
            var startWorldCoordinates =
                InterpolateVector(va.WorldCoordinates, vb.WorldCoordinates, gradient1);
            var endWorldCoordinates =
                InterpolateVector(vc.WorldCoordinates, vd.WorldCoordinates, gradient2);

            var endX = (int)(endCoordinates.X+0.5);
            var startX = (int) startCoordinates.X;
            var gradient = 0d;
            var step = 1d / (endX - startX);

            for (var x = startX; x <= endX; x++, gradient += step)
            {
                var pixelCoordinates = InterpolateVector(startCoordinates, endCoordinates, gradient);
                var pixelNormal = InterpolateVector(startNormalVector, endNormalVector, gradient);
                var pixelWorldCoordinates = InterpolateVector(startWorldCoordinates, endWorldCoordinates, gradient);
                var color = Illumination.GetPixelColor(pixelNormal, viewerPosition, pixelWorldCoordinates);

                DrawPoint(new Point3D(x, currentY, pixelCoordinates.Z), color);
            }
        }
    }
}
