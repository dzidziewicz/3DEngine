using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MathNet.Numerics.LinearAlgebra.Double;
using SharpDX;
using SolarSystem3DEngine.LightSources;
using SolarSystem3DEngine.Shaders;

namespace SolarSystem3DEngine
{
    namespace SoftEngine
    {
        public unsafe class Device
        {
            private readonly int* _backBuffer;
            private readonly double[] _depthBuffer;
            private readonly WriteableBitmap _bmp;
            private readonly object[] _lockBuffer;
            private readonly int _renderWidth;
            private readonly int _renderHeight;
            private readonly LightBase[] _pointLights;
            private readonly ShaderBase _shader;

            public Device(WriteableBitmap bmp, LightBase[] pointLights, ShaderBase shader)
            {
                _bmp = bmp;
                _pointLights = pointLights;
                _shader = shader;
                _shader.DrawPoint = DrawPoint;
                _renderHeight = bmp.PixelHeight;
                _renderWidth = bmp.PixelWidth;
                _backBuffer = (int*)_bmp.BackBuffer.ToPointer();//new int[renderWidth*renderHeight];//
                _depthBuffer = new double[_renderWidth * _renderHeight];
                _lockBuffer = new object[_renderWidth * _renderHeight];
                for (var i = 0; i < _lockBuffer.Length; i++)
                {
                    _lockBuffer[i] = new object();
                }

                //                    = DenseMatrix.OfArray(new double[,]
                //                {
                //                    {Math.Cos(phi), -Math.Sin(phi), 0, Math.Sin(phi)},
                //                    {Math.Sin(phi), Math.Cos(phi), 0, Math.Cos(phi)},
                //                    {0, 0, 1, 0},
                //                    {0, 0, 0, 1}
                //                });
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

            private Vertex InvalidatePoint(Vertex vertex, DenseMatrix viewModelMatrix,
                DenseMatrix projectionViewModelMatrix, DenseMatrix normalMatrix)
            {
                var vectorCoordinates = DenseMatrix.OfArray(new[,]
                {
                    {vertex.Coordinates.X},
                    {vertex.Coordinates.Y},
                    {vertex.Coordinates.Z},
                    {vertex.Coordinates.W}
                });
                var vectorNormal = DenseMatrix.OfArray(new[,]
                {
                    {vertex.Normal.X},
                    {vertex.Normal.Y},
                    {vertex.Normal.Z},
                    {vertex.Normal.W}
                });
                var pprim = projectionViewModelMatrix * vectorCoordinates;
                var w = pprim[3, 0];
                var newCoordinates = new Point3D(pprim) / w;
                newCoordinates = Computations.Scale(newCoordinates, _renderWidth, _renderHeight);

                var point3DWorld = viewModelMatrix * vectorCoordinates;
                var new3DWorld = new Point3D(point3DWorld);

                var normal3DWorld = normalMatrix * vectorNormal;
                var newNormal = new Point3D(normal3DWorld);
                newNormal = newNormal / newNormal.W;

                return new Vertex { Coordinates = newCoordinates, Normal = newNormal, WorldCoordinates = new3DWorld };
            }

            public void DrawPoint(Point3D point, Color color)
            {
                if (point.X >= 0 && point.Y >= 0 && point.X < _renderWidth && point.Y < _renderHeight)
                    PutPixel(point, color);
            }

            // The main method of the engine that re-compute each vertex projection during each frame

            public void Render(Camera camera, IEnumerable<Mesh> meshes)
            {
                foreach (var mesh in meshes)
                {
                    _shader.Illumination.SetCoeffitients(mesh.KAmbient, mesh.KDiffuse, mesh.KSpecular);
                    //var faceIndex = 0;
                    Parallel.For(0, mesh.Faces.Length, faceIndex =>
                    //foreach(var face in mesh.Faces)
                    {
                        var face = mesh.Faces[faceIndex];

                        var vertexA = mesh.Vertices[face.A];
                        var vertexB = mesh.Vertices[face.B];
                        var vertexC = mesh.Vertices[face.C];

                        var pixelA = InvalidatePoint(vertexA, mesh.ViewModelMatrix, mesh.ProjectionViewModelMatrix, mesh.NormalMatrix);
                        var pixelB = InvalidatePoint(vertexB, mesh.ViewModelMatrix, mesh.ProjectionViewModelMatrix, mesh.NormalMatrix);
                        var pixelC = InvalidatePoint(vertexC, mesh.ViewModelMatrix, mesh.ProjectionViewModelMatrix, mesh.NormalMatrix);

                        _shader.DrawTriangle(pixelA, pixelB, pixelC);
                        //faceIndex++;
                    });
                }
            }
        }
    }
}
