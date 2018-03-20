using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MathNet.Numerics.LinearAlgebra.Double;
using SharpDX;

namespace SolarSystem3DEngine
{
    namespace SoftEngine
    {
        public unsafe class Device
        {
            private readonly int* _backBuffer;
            private readonly double[] _depthBuffer;
            private readonly WriteableBitmap _bmp;
            private readonly DenseMatrix _modelMatrix;
            private readonly DenseMatrix _transformationMatrix;
            private readonly object[] _lockBuffer;
            private readonly int _renderWidth;
            private readonly int _renderHeight;
            private readonly DenseMatrix _viewMatrix;
            private readonly DenseMatrix _projectionMatrix;

            public Device(WriteableBitmap bmp, double phi, ViewMatrixConfiguration configuration, ProjectionMatrixConfiguration projConfiguration)
            {
                _bmp = bmp;
                _renderHeight = bmp.PixelHeight;
                _renderWidth = bmp.PixelWidth;
                _backBuffer = (int*)_bmp.BackBuffer.ToPointer();//new int[renderWidth*renderHeight];//
                _depthBuffer = new double[_renderWidth * _renderHeight];
                _lockBuffer = new object[_renderWidth * _renderHeight];
                for (var i = 0; i < _lockBuffer.Length; i++)
                {
                    _lockBuffer[i] = new object();
                }

                _modelMatrix = DenseMatrix.OfArray(new double[,]
                {
                    {Math.Cos(phi), -Math.Sin(phi), 0, 0.5*Math.Sin(phi)},
                    {Math.Sin(phi), Math.Cos(phi), 0, 0.5*Math.Sin(phi)},
                    {0, 0, 1, 0.5*Math.Sin(phi)},
                    {0, 0, 0, 1}
                });
                _viewMatrix = configuration.ViewMatrix;
                _projectionMatrix = projConfiguration.ProjectionMatrix;
                _transformationMatrix = _projectionMatrix * _viewMatrix * _modelMatrix;
            }

            public void Clear(byte r, byte g, byte b, byte a)
            {
                for (var index = 0; index < _renderWidth * _renderHeight; index += 1)
                {
                    _backBuffer[index] = Computations.ConvertToArgb32(Color.FromArgb(255, r, g, b));
                    _depthBuffer[index] = float.MaxValue;
                }
            }

            public void Present()
            {
                // request a redraw of the entire bitmap
                _bmp.AddDirtyRect(new Int32Rect(0, 0, _renderWidth, _renderHeight));
                //int stride = 4 * ((renderWidth * 4 + 3) / 4);
                //_bmp.WritePixels(new Int32Rect(0, 0, renderWidth, renderHeight), _backBuffer, stride, 0);
            }

            // Called to put a pixel on screen at a specific X,Y coordinates
            public void PutPixel(Point3D point, Color color)
            {
                var index = (int)point.X + (int)point.Y * _renderWidth;
                lock (_lockBuffer[index])
                {
                    if (_depthBuffer[index] < point.Z) return; // Discard

                    _backBuffer[index] = Computations.ConvertToArgb32(color);
                    _depthBuffer[index] = point.Z;
                }
            }

            // DrawPoint calls PutPixel but does the clipping operation before
            public void DrawPoint(Point3D point, Color color)
            {
                // Clipping what's visible on screen
                if (point.X >= 0 && point.Y >= 0 && point.X < _renderWidth && point.Y < _renderHeight)
                    PutPixel(point, color);
            }

            // The main method of the engine that re-compute each vertex projection during each frame
            public void Render(Camera camera, params Mesh[] meshes)
            {
                foreach (var mesh in meshes)
                {
                    //var faceIndex = 0;
                    Parallel.For(0, mesh.Faces.Length, faceIndex =>
                    //foreach(var face in mesh.Faces)
                    {
                        var face = mesh.Faces[faceIndex];

                        var vertexA = mesh.Vertices[face.A];
                        var vertexB = mesh.Vertices[face.B];
                        var vertexC = mesh.Vertices[face.C];

                        var pixelA = InvalidatePoint(vertexA);
                        var pixelB = InvalidatePoint(vertexB);
                        var pixelC = InvalidatePoint(vertexC);

                        DrawTriangle(pixelA, pixelB, pixelC, Color.FromArgb(255, 255, 0, 0));
                        //faceIndex++;
                    });
                }
            }

            private Vertex InvalidatePoint(Vertex vertex)
            {
                var vectorCoordinates = DenseMatrix.OfArray(new [,]
                {
                    {vertex.Coordinates.X},
                    {vertex.Coordinates.Y},
                    {vertex.Coordinates.Z},
                    {vertex.Coordinates.W}
                });
                var vectorNormal = DenseMatrix.OfArray(new [,]
                {
                    {vertex.Normal.X},
                    {vertex.Normal.Y},
                    {vertex.Normal.Z},
                    {vertex.Normal.W}
                });
                var pprim = _transformationMatrix * vectorCoordinates;
                var w = pprim[3, 0];
                var point3DWorld = _modelMatrix * vectorCoordinates;
                var normal3DWorld = _modelMatrix * vectorNormal;
                var newCoordinates = new Point3D(pprim) / w; 
                
                newCoordinates = Computations.Scale(newCoordinates, _renderWidth, _renderHeight);
                var newNormal = new Point3D(normal3DWorld);
                var new3DWorld = new Point3D(point3DWorld);
                return new Vertex { Coordinates = newCoordinates, Normal = newNormal, WorldCoordinates = new3DWorld };
            }

            //public void DrawTriangle(Point3D p1, Point3D p2, Point3D p3, Color color)
            public void DrawTriangle(Vertex v1, Vertex v2, Vertex v3, Color color)
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

                // normal face's vector is the average normal between each vertex's normal
                // computing also the center point of the face
                ///var vnFace = (Vector3)(v1.Normal + v2.Normal + v3.Normal) / 3;
                ///var centerPoint = (Vector3)(v1.WorldCoordinates + v2.WorldCoordinates + v3.WorldCoordinates) / 3;
                // Light position 
                var lightPos = new Vector3(3, 1, 0);
                // computing the cos of the angle between the light vector and the normal vector
                // it will return a value between 0 and 1 that will be used as the intensity of the color
                ///var ndotl = ComputeNDotL(centerPoint, vnFace, lightPos);
                var nl1 = Computations.ComputeNDotL(v1.WorldCoordinates, v1.Normal, lightPos);
                var nl2 = Computations.ComputeNDotL(v2.WorldCoordinates, v2.Normal, lightPos);
                var nl3 = Computations.ComputeNDotL(v3.WorldCoordinates, v3.Normal, lightPos);

                var data = new ScanLineData();

                // inverse slopes
                var dP1P2 = (p2.Y - p1.Y > 0) ? (p2.X - p1.X) / (p2.Y - p1.Y) : 0;

                var dP1P3 = (p3.Y - p1.Y > 0) ? (p3.X - p1.X) / (p3.Y - p1.Y) : 0;

                // First case where P2 is on the right of P1P3
                if (dP1P2 > dP1P3)
                {
                    for (var y = (int)p1.Y; y < (int)p2.Y; y++)
                    {
                        data.currentY = y;
                        data.ndotla = nl1;
                        data.ndotlb = nl3;
                        data.ndotlc = nl1;
                        data.ndotld = nl2;
                        ProcessScanLine(data, v1, v3, v1, v2, color);
                    }

                    for (var y = (int)p2.Y; y <= (int)p3.Y; y++)
                    {
                        data.currentY = y;
                        data.ndotla = nl1;
                        data.ndotlb = nl3;
                        data.ndotlc = nl2;
                        data.ndotld = nl3;
                        ProcessScanLine(data, v1, v3, v2, v3, color);
                    }
                }
                // First case where P2 is on the left of P1P3
                else
                {
                    for (var y = (int)p1.Y; y < (int)p2.Y; y++)
                    {
                        data.currentY = y;
                        data.ndotla = nl1;
                        data.ndotlb = nl2;
                        data.ndotlc = nl1;
                        data.ndotld = nl3;
                        ProcessScanLine(data, v1, v2, v1, v3, color);
                    }

                    for (var y = (int)p2.Y; y <= (int)p3.Y; y++)
                    {
                        data.currentY = y;
                        data.ndotla = nl2;
                        data.ndotlb = nl3;
                        data.ndotlc = nl1;
                        data.ndotld = nl3;
                        ProcessScanLine(data, v2, v3, v1, v3, color);
                    }
                }
            }

            // drawing line between 2 points from left to right
            // papb -> pcpd
            // pa, pb, pc, pd must then be sorted before
            private void ProcessScanLine(ScanLineData data, Vertex va, Vertex vb, Vertex vc, Vertex vd, Color color)
            {
                var pa = va.Coordinates;
                var pb = vb.Coordinates;
                var pc = vc.Coordinates;
                var pd = vd.Coordinates;

                // Thanks to current Y, we can compute the gradient to compute others values like
                // the starting X (sx) and ending X (ex) to draw between
                // if pa.Y == pb.Y or pc.Y == pd.Y, gradient is forced to 1
                var gradient1 = pa.Y != pb.Y ? (data.currentY - pa.Y) / (pb.Y - pa.Y) : 1;
                var gradient2 = pc.Y != pd.Y ? (data.currentY - pc.Y) / (pd.Y - pc.Y) : 1;

                var sx = (int)Computations.Interpolate(pa.X, pb.X, gradient1);
                var ex = (int)Computations.Interpolate(pc.X, pd.X, gradient2);

                // starting Z & ending Z
                var z1 = Computations.Interpolate(pa.Z, pb.Z, gradient1);
                var z2 = Computations.Interpolate(pc.Z, pd.Z, gradient2);

                var snl = Computations.Interpolate(data.ndotla, data.ndotlb, gradient1);
                var enl = Computations.Interpolate(data.ndotlc, data.ndotld, gradient2);

                // drawing a line from left (sx) to right (ex) 
                for (var x = sx; x < ex; x++)
                {
                    var gradient = (x - sx) / (float)(ex - sx);

                    var z = Computations.Interpolate(z1, z2, gradient);
                    var ndotl = Computations.Interpolate(snl, enl, gradient); //data.ndotla;
                    // changing the color value using the cosine of the angle
                    // between the light vector and the normal vector
                    DrawPoint(new Point3D(x, data.currentY, z), color * (float)ndotl);
                }
            }
        }
    }
}
