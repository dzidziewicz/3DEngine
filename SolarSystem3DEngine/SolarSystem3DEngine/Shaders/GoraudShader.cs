using System.Numerics;
using SolarSystem3DEngine.Illuminations;

namespace SolarSystem3DEngine.Shaders
{
    public class GoraudShader: ShaderBase
    {
        public GoraudShader(BaseIllumination illumination) : base(illumination)
        {
        }

        public override void DrawTriangle(Vertex v1, Vertex v2, Vertex v3)
        {
            SortVertices(ref v1, ref v2, ref v3);

            var p1 = v1.Coordinates;
            var p2 = v2.Coordinates;
            var p3 = v3.Coordinates;

            var viewerPosition =  new Vector3(320, 320, 0);// Vector3.Zero;new Vector3(0.1f, 0f, -15);//
            var color1 = Illumination.GetPixelColor(v1.Normal, viewerPosition, v1.Coordinates);
            var color2 = Illumination.GetPixelColor(v2.Normal, viewerPosition, v2.Coordinates);
            var color3 = Illumination.GetPixelColor(v3.Normal, viewerPosition, v3.Coordinates);

            var data = new GoraudScanLineData();

            // inverse slopes
            var dP1P2 = (p2.Y - p1.Y > 0) ? (p2.X - p1.X) / (p2.Y - p1.Y) : 0;

            var dP1P3 = (p3.Y - p1.Y > 0) ? (p3.X - p1.X) / (p3.Y - p1.Y) : 0;

            // First case where P2 is on the right of P1P3
            if (dP1P2 > dP1P3)
            {
                var endY = (int)(p2.Y + 0.5);

                for (var y = (int)p1.Y; y < endY; y++)
                {
                    data.CurrentY = y;
                    data.ColorA = color1;
                    data.ColorB = color3;
                    data.ColorC = color1;
                    data.ColorD = color2;
                    ProcessScanLine(data, v1, v3, v1, v2);
                }
                endY = (int)p3.Y;
                for (var y = (int)p2.Y; y <= endY; y++)
                {
                    data.CurrentY = y;
                    data.ColorA = color1;
                    data.ColorB = color3;
                    data.ColorC = color2;
                    data.ColorD = color3;
                    ProcessScanLine(data, v1, v3, v2, v3);
                }
            }
            // First case where P2 is on the left of P1P3
            else
            {
                var endY = (int)(p2.Y + 0.5);

                for (var y = (int)p1.Y; y < endY; y++)
                {
                    data.CurrentY = y;
                    data.ColorA = color1;
                    data.ColorB = color2;
                    data.ColorC = color1;
                    data.ColorD = color3;
                    ProcessScanLine(data, v1, v2, v1, v3);
                }
                endY = (int)p3.Y;
                for (var y = (int)p2.Y; y <= endY; y++)
                {
                    data.CurrentY = y;
                    data.ColorA = color2;
                    data.ColorB = color3;
                    data.ColorC = color1;
                    data.ColorD = color3;
                    ProcessScanLine(data, v2, v3, v1, v3);
                }
            }
        }

        // drawing line between 2 points from left to right
        // papb -> pcpd
        // pa, pb, pc, pd must then be sorted before
        private void ProcessScanLine(GoraudScanLineData data, Vertex va, Vertex vb, Vertex vc, Vertex vd)
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

            var sx = (int)Computations.Interpolate(pa.X, pb.X, gradient1);
            var ex = (int)Computations.Interpolate(pc.X, pd.X, gradient2);

            // starting Z & ending Z
            var z1 = Computations.Interpolate(pa.Z, pb.Z, gradient1);
            var z2 = Computations.Interpolate(pc.Z, pd.Z, gradient2);

            var sxColor = InterpolateColor(data.ColorA, data.ColorB, gradient1);
            var exColor = InterpolateColor(data.ColorC, data.ColorD, gradient2);

            // drawing a line from left (sx) to right (ex) 
            var gradient = 0d;
            var step = 1d / (ex - sx);
            for (var x = sx; x < ex; x++, gradient += step)
            {
                var z = Computations.Interpolate(z1, z2, gradient);
                var color = InterpolateColor(sxColor, exColor, gradient); 

                DrawPoint(new Point3D(x, data.CurrentY, z), color);
            }
        }
        
    }
}
