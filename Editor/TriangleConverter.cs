using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.aoyon.triangleselector
{
    public static class TriangleConverter
    {
        public static Vector3[] Encode(Mesh mesh, int[] triangleIndices)
        {
            Vector3[] vertices = mesh.vertices;
            int[] triagnles = mesh.triangles;
            
            HashSet<Vector3> positions = new HashSet<Vector3>();

            for (int i = 0; i < triangleIndices.Length; i++)
            {
                int triangleIndex = triangleIndices[i];
                positions.Add(vertices[triagnles[triangleIndex * 3]]);
                positions.Add(vertices[triagnles[triangleIndex * 3 + 1]]);
                positions.Add(vertices[triagnles[triangleIndex * 3 + 2]]);
            }

            return positions.ToList().ToArray();
        }

        public static int[] Decode(Mesh mesh, Vector3[] positions)
        {   
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            HashSet<Vector3> positionSet = new HashSet<Vector3>(positions); 

            List<int> triangleIndices = new List<int>();

            for (int j = 0; j < triangles.Length; j += 3)
            {
                if (positionSet.Contains(vertices[triangles[j]]) &&
                    positionSet.Contains(vertices[triangles[j + 1]]) &&
                    positionSet.Contains(vertices[triangles[j + 2]]))
                {
                    triangleIndices.Add(j / 3);
                }
            }

            return triangleIndices.ToArray();
        }

    }
}