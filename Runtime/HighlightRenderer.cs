using UnityEngine;
using System.Collections.Generic;
using System;

namespace com.aoyon.triangleselector
{
    public class HighlightRenderer : MonoBehaviour
    {
        [HideInInspector] public List<Vector3> linePoints = new List<Vector3>();
        [HideInInspector] public Color highlightColor = Color.red;

        void OnDrawGizmos()
        {
            Gizmos.color = highlightColor;
            for (int i = 0; i < linePoints.Count - 1; i += 2)
            {
                Gizmos.DrawLine(linePoints[i], linePoints[i + 1]);
            }
        }
    }
}