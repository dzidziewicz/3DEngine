using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Newtonsoft.Json;

namespace SolarSystem3DEngine
{
    public static class LoadMeshes
    {
        // Loading the JSON file in an asynchronous manner
        public static Mesh[] LoadJsonFileAsync(string fileName)
        {
            var meshes = new List<Mesh>();
            var data = File.ReadAllText(fileName);
            dynamic jsonObject = JsonConvert.DeserializeObject(data);

            for (var meshIndex = 0; meshIndex < jsonObject.meshes.Count; meshIndex++)
            {
                var verticesArray = jsonObject.meshes[meshIndex].vertices;
                // Faces
                var indicesArray = jsonObject.meshes[meshIndex].indices;

                var uvCount = jsonObject.meshes[meshIndex].uvCount.Value;
                var verticesStep = 1;

                // Depending of the number of texture's coordinates per vertex
                // we're jumping in the vertices array  by 6, 8 & 10 windows frame
                switch ((int)uvCount)
                {
                    case 0:
                        verticesStep = 6;
                        break;
                    case 1:
                        verticesStep = 8;
                        break;
                    case 2:
                        verticesStep = 10;
                        break;
                }

                // the number of interesting vertices information for us
                var verticesCount = verticesArray.Count / verticesStep;
                // number of faces is logically the size of the array divided by 3 (A, B, C)
                var facesCount = indicesArray.Count / 3;
                var mesh = new Mesh(jsonObject.meshes[meshIndex].name.Value, verticesCount, facesCount);

                // Filling the Vertices array of our mesh first
                for (var index = 0; index < verticesCount; index++)
                {
                    var x = (float)verticesArray[index * verticesStep].Value;
                    var y = (float)verticesArray[index * verticesStep + 1].Value;
                    var z = (float)verticesArray[index * verticesStep + 2].Value;
                    // Loading the vertex normal exported by Blender
                    var nx = (float)verticesArray[index * verticesStep + 3].Value;
                    var ny = (float)verticesArray[index * verticesStep + 4].Value;
                    var nz = (float)verticesArray[index * verticesStep + 5].Value;
                    mesh.Vertices[index] = new Vertex { Coordinates = new Point3D(x, y, z), Normal = new Point3D(nx, ny, nz) };
                }

                // Then filling the Faces array
                for (var index = 0; index < facesCount; index++)
                {
                    var a = (int)indicesArray[index * 3].Value;
                    var b = (int)indicesArray[index * 3 + 1].Value;
                    var c = (int)indicesArray[index * 3 + 2].Value;
                    mesh.Faces[index] = new Face(a, b, c);
                }

                // Getting the position you've set in Blender
                var position = jsonObject.meshes[meshIndex].position;
                mesh.Position = new Vector3((float)position[0].Value, (float)position[1].Value, (float)position[2].Value);
                meshes.Add(mesh);
            }
            return meshes.ToArray();
        }
    }
}
