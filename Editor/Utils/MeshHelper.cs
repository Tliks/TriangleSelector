using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace com.aoyon.triangleselector.utils
{
    public class MeshHelper
    {

        public static Mesh KeepMesh(Mesh originalMesh, IEnumerable<int> triangleIndexestoKeep)
        {
            return GetMesh(originalMesh, triangleIndexestoKeep, true);
        }

        public static Mesh DeleteMesh(Mesh originalMesh, IEnumerable<int> triangleIndexestoRemove)
        {
            return GetMesh(originalMesh, triangleIndexestoRemove, false);
        }

        public static Mesh KeepMesh(Mesh originalMesh, IEnumerable<Vector3> positiontoKeep)
        {
            var triangleIndexes = TriangleConverter.Decode(originalMesh, positiontoKeep);
            return GetMesh(originalMesh, triangleIndexes, true);
        }

        public static Mesh DeleteMesh(Mesh originalMesh, IEnumerable<Vector3> positionToDelete)
        {
            var triangleIndexes = TriangleConverter.Decode(originalMesh, positionToDelete);
            return GetMesh(originalMesh, triangleIndexes, false);
        }
        
        private static Mesh GetMesh(Mesh originalMesh, IEnumerable<int> triangleIndexes, bool keep)
        {
            var triangleset = new HashSet<int>(triangleIndexes);
            HashSet<int> verticesToKeep = new();
            List<int>[] newSubmeshTriangles = new List<int>[originalMesh.subMeshCount];
            for (int i = 0; i < originalMesh.subMeshCount; ++i)
                newSubmeshTriangles[i] = new List<int>();

            int globalTriangleIndexOffset = 0;
            for (int subMeshIndex = 0; subMeshIndex < originalMesh.subMeshCount; subMeshIndex++)
            {
                int[] originalTriangles = originalMesh.GetTriangles(subMeshIndex);
                for (int i = 0; i < originalTriangles.Length; i += 3)
                {
                    int globalTriangleIndex = (globalTriangleIndexOffset + i) / 3;
                    if ((keep && triangleset.Contains(globalTriangleIndex)) || 
                        (!keep && !triangleset.Contains(globalTriangleIndex)))
                    {
                        newSubmeshTriangles[subMeshIndex].Add(originalTriangles[i]);
                        newSubmeshTriangles[subMeshIndex].Add(originalTriangles[i + 1]);
                        newSubmeshTriangles[subMeshIndex].Add(originalTriangles[i + 2]);

                        verticesToKeep.Add(originalTriangles[i]);
                        verticesToKeep.Add(originalTriangles[i + 1]);
                        verticesToKeep.Add(originalTriangles[i + 2]);
                    }
                }
                globalTriangleIndexOffset += originalTriangles.Length;
            }

            int newVerticesCount = verticesToKeep.Count;
            List<Vector3> newVerticesList = new(newVerticesCount);
            List<Vector3> newNormalsList = new(newVerticesCount);
            List<Vector4> newTangentsList = new(newVerticesCount);
            List<Vector2> newUvList = new(newVerticesCount);
            List<Vector2> newUv2List = new();
            List<Vector2> newUv3List = new();
            List<Vector2> newUv4List = new();
            List<BoneWeight> newBoneWeightsList = new(newVerticesCount);

            Dictionary<int, int> oldToNewVertexMap = new(newVerticesCount);

            Vector3[] vertices = originalMesh.vertices;
            Vector3[] normals = originalMesh.normals;
            Vector4[] tangents = originalMesh.tangents;
            Vector2[] uv = originalMesh.uv;
            Vector2[] uv2 = originalMesh.uv2;
            Vector2[] uv3 = originalMesh.uv3;
            Vector2[] uv4 = originalMesh.uv4;
            BoneWeight[] boneWeights = originalMesh.boneWeights;

            foreach (int oldIndex in verticesToKeep)
            {
                int newIndex = newVerticesList.Count;
                newVerticesList.Add(vertices[oldIndex]);
                if (normals.Length > oldIndex) newNormalsList.Add(normals[oldIndex]);
                if (tangents.Length > oldIndex) newTangentsList.Add(tangents[oldIndex]);
                if (uv.Length > oldIndex) newUvList.Add(uv[oldIndex]);
                if (uv2.Length > oldIndex) newUv2List.Add(uv2[oldIndex]);
                if (uv3.Length > oldIndex) newUv3List.Add(uv3[oldIndex]);
                if (uv4.Length > oldIndex) newUv4List.Add(uv4[oldIndex]);
                if (boneWeights.Length > oldIndex) newBoneWeightsList.Add(boneWeights[oldIndex]);

                oldToNewVertexMap[oldIndex] = newIndex;
            }

            Mesh newMesh = new Mesh();
            newMesh.vertices = newVerticesList.ToArray();
            newMesh.normals = newNormalsList.ToArray();
            newMesh.tangents = newTangentsList.ToArray();
            newMesh.uv = newUvList.ToArray();
            if (newUv2List.Count > 0) newMesh.uv2 = newUv2List.ToArray();
            if (newUv3List.Count > 0) newMesh.uv3 = newUv3List.ToArray();
            if (newUv4List.Count > 0) newMesh.uv4 = newUv4List.ToArray();
            newMesh.boneWeights = newBoneWeightsList.ToArray();

            newMesh.subMeshCount = originalMesh.subMeshCount;

            for (int subMeshIndex = 0; subMeshIndex < newMesh.subMeshCount; subMeshIndex++)
            {
                List<int> newTriangles = new();
                foreach (int oldVertexIndex in newSubmeshTriangles[subMeshIndex])
                {
                    newTriangles.Add(oldToNewVertexMap[oldVertexIndex]);
                }
                newMesh.SetTriangles(newTriangles.ToArray(), subMeshIndex);
            }

            CopyColors(originalMesh, newMesh, oldToNewVertexMap);
            CopyBlendShapes(originalMesh, newMesh, oldToNewVertexMap);
            newMesh.bindposes = originalMesh.bindposes;

            return newMesh;
        }

        private static void CopyColors(Mesh originalMesh, Mesh newMesh, Dictionary<int, int> indexMap)
        {
            if (originalMesh.colors != null && originalMesh.colors.Length > 0)
            {
                Color[] newColors = new Color[newMesh.vertexCount];
                foreach (var kv in indexMap)
                {
                    newColors[kv.Value] = originalMesh.colors[kv.Key];
                }
                newMesh.colors = newColors;
            }
            else if (originalMesh.colors32 != null && originalMesh.colors32.Length > 0)
            {
                Color32[] newColors32 = new Color32[newMesh.vertexCount];
                foreach (var kv in indexMap)
                {
                    newColors32[kv.Value] = originalMesh.colors32[kv.Key];
                }
                newMesh.colors32 = newColors32;
            }
        }

        private static void CopyBlendShapes(Mesh originalMesh, Mesh newMesh, Dictionary<int, int> indexMap)
        {
            for (int i = 0; i < originalMesh.blendShapeCount; i++)
            {
                string blendShapeName = originalMesh.GetBlendShapeName(i);
                int frameCount = originalMesh.GetBlendShapeFrameCount(i);

                for (int j = 0; j < frameCount; j++)
                {
                    float frameWeight = originalMesh.GetBlendShapeFrameWeight(i, j);
                    Vector3[] frameVertices = new Vector3[originalMesh.vertexCount];
                    Vector3[] frameNormals = new Vector3[originalMesh.vertexCount];
                    Vector3[] frameTangents = new Vector3[originalMesh.vertexCount];

                    originalMesh.GetBlendShapeFrameVertices(i, j, frameVertices, frameNormals, frameTangents);

                    Vector3[] newFrameVertices = new Vector3[newMesh.vertexCount];
                    Vector3[] newFrameNormals = new Vector3[newMesh.vertexCount];
                    Vector3[] newFrameTangents = new Vector3[newMesh.vertexCount];

                    foreach (var kv in indexMap)
                    {
                        int originalIndex = kv.Key;
                        int newIndex = kv.Value;
                        newFrameVertices[newIndex] = frameVertices[originalIndex];
                        newFrameNormals[newIndex] = frameNormals[originalIndex];
                        newFrameTangents[newIndex] = frameTangents[originalIndex];
                    }

                    newMesh.AddBlendShapeFrame(blendShapeName, frameWeight, newFrameVertices, newFrameNormals, newFrameTangents);
                }
            }
        }

        public static void RemoveUnusedMaterials(SkinnedMeshRenderer skinnedMeshRenderer)
        {
            Mesh newMesh = skinnedMeshRenderer.sharedMesh;
            HashSet<int> usedMaterialIndices = new HashSet<int>();

            for (int subMeshIndex = 0; subMeshIndex < newMesh.subMeshCount; subMeshIndex++)
            {
                int[] triangles = newMesh.GetTriangles(subMeshIndex);
                if (triangles.Length > 0)
                {
                    usedMaterialIndices.Add(subMeshIndex);
                }
            }

            List<Material> usedMaterials = new List<Material>();
            for (int i = 0; i < skinnedMeshRenderer.sharedMaterials.Length; i++)
            {
                if (usedMaterialIndices.Contains(i))
                {
                    usedMaterials.Add(skinnedMeshRenderer.sharedMaterials[i]);
                }
            }

            skinnedMeshRenderer.sharedMaterials = usedMaterials.ToArray();
        }

        public static Mesh keepTriangles(Mesh originalMesh, HashSet<int> triangleIndexestoKeep)
        {
            return ProcessTriangles(originalMesh, triangleIndexestoKeep, true);
        }

        public static Mesh RemoveTriangles(Mesh originalMesh, HashSet<int> triangleIndexestoRemove)
        {
            return ProcessTriangles(originalMesh, triangleIndexestoRemove, false);
        }

        private static Mesh ProcessTriangles(Mesh originalMesh, HashSet<int> triangleIndexes, bool keep)
        {
            Mesh newMesh = Object.Instantiate(originalMesh);
            
            int submeshCount = originalMesh.subMeshCount;
            List<int>[] newSubmeshTriangles = new List<int>[submeshCount];

            int globalTriangleIndexOffset = 0;
            for (int submesh = 0; submesh < submeshCount; submesh++)
            {
                int[] originalTriangles = originalMesh.GetTriangles(submesh);
                newSubmeshTriangles[submesh] = new List<int>();

                for (int i = 0; i < originalTriangles.Length; i += 3)
                {
                    int globalTriangleIndex = (globalTriangleIndexOffset + i) / 3;

                    if ((keep && triangleIndexes.Contains(globalTriangleIndex)) || 
                        (!keep && !triangleIndexes.Contains(globalTriangleIndex)))
                    {
                        newSubmeshTriangles[submesh].Add(originalTriangles[i]);
                        newSubmeshTriangles[submesh].Add(originalTriangles[i + 1]);
                        newSubmeshTriangles[submesh].Add(originalTriangles[i + 2]);
                    }
                }

                globalTriangleIndexOffset += originalTriangles.Length;
            }

            newMesh.subMeshCount = submeshCount;
            for (int submesh = 0; submesh < submeshCount; submesh++)
            {
                newMesh.SetTriangles(newSubmeshTriangles[submesh], submesh);
            }

            return newMesh;
        }
        
        public static (Mesh, Dictionary<int, int>) ProcesscolliderMesh(Mesh originalMesh, HashSet<int> trianglesToKeep)
        {
            Mesh newMesh = Object.Instantiate(originalMesh);
            int[] originalTriangles = newMesh.triangles;

            // trianglesToKeepのサイズに基づいて初期サイズを設定
            List<int> newTriangles = new List<int>(trianglesToKeep.Count * 6);
            Dictionary<int, int> newToOldTriangleMap = new Dictionary<int, int>(trianglesToKeep.Count * 2);

            int newTriangleIndex = 0;
            for (int i = 0; i < originalTriangles.Length; i += 3)
            {
                int triangleIndex = i / 3;

                if (trianglesToKeep.Contains(triangleIndex))
                {
                    // 前面ポリゴンの追加
                    for (int j = 0; j < 3; j++)
                    {
                        newTriangles.Add(originalTriangles[i + j]);
                    }
                    newToOldTriangleMap[newTriangleIndex] = triangleIndex;
                    newTriangleIndex++;

                    // 裏面ポリゴンの追加
                    newTriangles.Add(originalTriangles[i + 2]);
                    newTriangles.Add(originalTriangles[i + 1]);
                    newTriangles.Add(originalTriangles[i]);
                    newToOldTriangleMap[newTriangleIndex] = triangleIndex;
                    newTriangleIndex++;
                }
            }

            newMesh.triangles = newTriangles.ToArray();

            return (newMesh, newToOldTriangleMap);
        }

        public static int ConvertNewTriangleIndexToOld(int newTriangleIndex, Dictionary<int, int> newToOldTriangleMap)
        {
            if (newToOldTriangleMap.TryGetValue(newTriangleIndex, out int oldTriangleIndex))
            {
                return oldTriangleIndex;
            }
            return -1; // 対応する元のトライアングルが見つからない場合
        }

        public static HashSet<int> GetPolygonVertexIndices(Mesh mesh, HashSet<int> triangleIndices)
        {
            // メッシュの三角形と頂点配列を取得
            int[] triangles = mesh.triangles;
            Vector3[] vertices = mesh.vertices;

            if (triangles == null || triangleIndices == null || vertices == null) return null;

            HashSet<int> vertexIndices = new HashSet<int>();

            foreach (int triangleIndex in triangleIndices)
            {
                if (triangleIndex < 0 || triangleIndex >= triangles.Length / 3)
                {
                    continue;
                }

                int index0 = triangles[triangleIndex * 3];
                int index1 = triangles[triangleIndex * 3 + 1];
                int index2 = triangles[triangleIndex * 3 + 2];

                vertexIndices.Add(index0);
                vertexIndices.Add(index1);
                vertexIndices.Add(index2);
            }

            return vertexIndices;
        }


    }
}