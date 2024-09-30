using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using com.aoyon.triangleselector.utils;

namespace com.aoyon.triangleselector
{


    public struct Island
    {
        public List<int> VertexIndices { get; }
        public List<int> TriangleIndices { get; }

        public Island(List<int> vertexIndices, List<int> triangleIndices)
        {
            VertexIndices = vertexIndices;
            TriangleIndices = triangleIndices;
        }
    }

    public class IslandHandler
    {
        private int[] Triangles { get; }
        private Vector3[] Vertices { get; }
        private List<Island> MergedIslands { get; }
        private List<Island> UnMergedIslands { get; }

        public IslandHandler(Mesh mesh)
        {
            Triangles = mesh.triangles;
            Vertices = mesh.vertices;
            (UnMergedIslands, MergedIslands) = GetIslands();
        }

        public (List<Island>, List<Island>) GetIslands()
        {
            int triCount = Triangles.Length;
            int vertCount = Vertices.Length;

            UnionFind unionFind = new UnionFind(vertCount);

            // 三角形の頂点をUnionFindで結合
            for (int i = 0; i < triCount; i += 3)
            {
                unionFind.Unite(Triangles[i], Triangles[i + 1]);
                unionFind.Unite(Triangles[i + 1], Triangles[i + 2]);
            }

            // 結合前のアイランドを作成
            Dictionary<int, Island> islandDict = new Dictionary<int, Island>(vertCount);
            for (int i = 0; i < vertCount; i++)
            {
                int root = unionFind.Find(i);
                if (!islandDict.ContainsKey(root))
                {
                    islandDict[root] = new Island(new List<int>(), new List<int>());
                }
                islandDict[root].VertexIndices.Add(i);
            }

            // 三角形インデックスを追加
            for (int i = 0; i < triCount; i += 3)
            {
                int root = unionFind.Find(Triangles[i]);
                islandDict[root].TriangleIndices.Add(i / 3);
            }

            // 結合前のアイランドを作成
            List<Island> beforeMerge = new List<Island>(islandDict.Values);

            // 同一座標の頂点を結合
            Dictionary<Vector3, int> vertexMap = new Dictionary<Vector3, int>(vertCount);
            for (int i = 0; i < vertCount; i++)
            {
                if (!vertexMap.TryAdd(Vertices[i], i))
                {
                    unionFind.Unite(vertexMap[Vertices[i]], i);
                }
            }
            // 結合後のアイランドを作成
            Dictionary<int, Island> mergedIslandDict = new Dictionary<int, Island>();
            foreach (var kvp in islandDict)
            {
                int mergedRoot = unionFind.Find(kvp.Key);
                if (!mergedIslandDict.TryGetValue(mergedRoot, out Island mergedIsland))
                {
                    mergedIsland = new Island(new List<int>(), new List<int>());
                    mergedIslandDict[mergedRoot] = mergedIsland;
                }
                mergedIsland.VertexIndices.AddRange(kvp.Value.VertexIndices);
                mergedIsland.TriangleIndices.AddRange(kvp.Value.TriangleIndices);
            }

            List<Island> afterMerge = new List<Island>(mergedIslandDict.Values);

            return (beforeMerge, afterMerge);
        }

        public HashSet<int> GetIslandtrianglesFromTriangleIndex(int triangleIndex, bool mergeSamePosition)
        {
            HashSet<int> foundTriangles = new HashSet<int>();
            List<Island> islands = mergeSamePosition ? MergedIslands : UnMergedIslands;

            foreach (var island in islands)
            {
                if (island.TriangleIndices.Contains(triangleIndex))
                {
                    foundTriangles = new HashSet<int>(island.TriangleIndices);
                    break;
                }
            }

            if (foundTriangles.Count == 0)
            {
            Debug.LogWarning($"No island found for triangleIndex: {triangleIndex}");
            }

            return foundTriangles;
        }

        public HashSet<int> GetIslandTrianglesInCollider(MeshCollider collider, bool mergeSamePosition, bool checkAll, Transform transform)
        {
            HashSet<int> foundVertices = new HashSet<int>();
            
            List<Island> islands = mergeSamePosition ? MergedIslands : UnMergedIslands;
            foreach (var island in islands)
            {
                if (IsInsideIsland(island, checkAll))
                {
                    foundVertices.UnionWith(island.TriangleIndices);
                }
            }

            return foundVertices;

            bool IsInsideIsland(Island island, bool checkAll) =>
                checkAll ? island.VertexIndices.TrueForAll(IsVertexInCollider) 
                        : island.VertexIndices.Exists(IsVertexInCollider);

            bool IsVertexInCollider(int vertexIndex)
            {
                Vector3 vertexWorldPos = transform.position + transform.rotation * Vertices[vertexIndex];
                Vector3 closestPoint = collider.ClosestPoint(vertexWorldPos);
                return closestPoint == vertexWorldPos;
            }
        }

        public HashSet<int> GetTrianglesNearPositionInIsland(int triangleIndex, Vector3 position, float threshold, Transform transform)
        {
            List<Island> islands = MergedIslands;
            Island? island = islands.FirstOrDefault(i => i.TriangleIndices.Contains(triangleIndex));
            
            if (island is null)
            {
                return new HashSet<int> { triangleIndex };
            }

            var foundTriangles = island.Value.TriangleIndices
                .Where(triIndex =>
                    Enumerable.Range(0, 3).All(i =>
                    {
                        int vertexIndex = Triangles[triIndex * 3 + i];
                        Vector3 vertexWorldPos = transform.position + transform.rotation * Vertices[vertexIndex];
                        return Vector3.Distance(position, vertexWorldPos) < threshold;
                    })
                )
                .ToHashSet();
            
            foundTriangles.Add(triangleIndex);

            return foundTriangles;
        }

        public HashSet<int> GetTrianglesInsideCollider(MeshCollider collider, Transform transform)
        {
            HashSet<int> insideTriangles = new HashSet<int>();

            for (int i = 0; i < Triangles.Length; i += 3)
            {
                bool isInside = true;
                for (int j = 0; j < 3; j++)
                {
                    int vertexIndex = Triangles[i + j];
                    Vector3 vertexWorldPos = transform.position + transform.rotation * Vertices[vertexIndex];
                    Vector3 closestPoint = collider.ClosestPoint(vertexWorldPos);
                    if (Vector3.Distance(closestPoint, vertexWorldPos) >= 0.001f)
                    {
                        isInside = false;
                        break;
                    }
                }
                
                if (isInside)
                {
                    insideTriangles.Add(i / 3);
                }
            }

            return insideTriangles;
        }

        public int GetIslandCount()
        {
            return UnMergedIslands.Count;
        }

        public int GetMergedIslandCount()
        {
            return MergedIslands.Count;
        }    

    }
}