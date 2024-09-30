using System.Collections.Generic;
using UnityEngine;

namespace com.aoyon.triangleselector
{
    public static class HighlightEdgesManager
    {
        private static List<Vector3> linePoints = new List<Vector3>();
        static HighlightRenderer selectedhighlightRenderer;
        static HighlightRenderer unselectedhighlightRenderer;

        public static void AddComponent(GameObject selectedObject, GameObject unselectedObject)
        {
            selectedhighlightRenderer = selectedObject.AddComponent<HighlightRenderer>();
            unselectedhighlightRenderer = unselectedObject.AddComponent<HighlightRenderer>();
        }

        public static void PrepareTriangleHighlights(int[] triangles, HashSet<int> triangleIndices, Vector3[] vertices, Transform origin)
        {
            if (triangles == null || triangleIndices == null || vertices == null || origin == null) return;

            linePoints.Clear();

            foreach (int triangleIndex in triangleIndices)
            {
                if (triangleIndex < 0 || triangleIndex >= triangles.Length / 3)
                {
                    Debug.LogError("Invalid triangle index.");
                    continue;
                }

                int index0 = triangles[triangleIndex * 3];
                int index1 = triangles[triangleIndex * 3 + 1];
                int index2 = triangles[triangleIndex * 3 + 2];

                Vector3 v0 = origin.position + origin.rotation * vertices[index0];
                Vector3 v1 = origin.position + origin.rotation * vertices[index1];
                Vector3 v2 = origin.position + origin.rotation * vertices[index2];

                linePoints.Add(v0); linePoints.Add(v1);
                linePoints.Add(v1); linePoints.Add(v2);
                linePoints.Add(v2); linePoints.Add(v0);

                UpdateLinepoints();
            }
        }

        public static void ClearHighlights()
        {
            linePoints.Clear();
            UpdateLinepoints();
        }

        public static void SetHighlightColor(Color color)
        {
            selectedhighlightRenderer.highlightColor = color;
            unselectedhighlightRenderer.highlightColor = color;
        }

        private static void UpdateLinepoints()
        {
            selectedhighlightRenderer.linePoints = linePoints;
            unselectedhighlightRenderer.linePoints = linePoints;
        }


    }
}