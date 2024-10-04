using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.aoyon.triangleselector
{
    public static class TriangleConverter
    {
        public static IEnumerable<Vector3> Encode(Mesh mesh, IEnumerable<int> triangleIndices)
        {
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            
            HashSet<Vector3> positions = new HashSet<Vector3>();

            foreach (int triangleIndex in triangleIndices)
            {
                positions.Add(vertices[triangles[triangleIndex * 3]]);
                positions.Add(vertices[triangles[triangleIndex * 3 + 1]]);
                positions.Add(vertices[triangles[triangleIndex * 3 + 2]]);
            }

            return positions;
        }

        public static IEnumerable<int> Decode(Mesh mesh, IEnumerable<Vector3> positions)
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

            return triangleIndices;
        }

    }
}