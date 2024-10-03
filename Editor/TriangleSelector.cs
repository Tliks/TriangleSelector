using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace com.aoyon.triangleselector
{

    public class TriangleSelectorResult
    {
        public List<int> SelectedTriangleIndices = new();
        public List<int> UnSelectedTriangleIndices = new();

        public string SelectionName = "";
        public SaveModes? SaveMode;
    }

    public static class TriangleSelector
    {
        public static bool Disposed = true;
        private static Action<TriangleSelectorResult> OnApply;
        private static PreviewController _previewController;
        
        public static void Initialize(
            SkinnedMeshRenderer skinnedMeshRenderer,
            SaveModes saveMode = SaveModes.New, 
            IReadOnlyCollection<int> defaultTriangleIndices = null,
            string defaultSelectionName = ""
        )
        {
            Dispose();
            Selection.activeObject = null;
            Selection.activeGameObject = null;
            _previewController = new PreviewController();
            defaultTriangleIndices ??= new List<int>();
            _previewController.Initialize(skinnedMeshRenderer, saveMode, defaultTriangleIndices, defaultSelectionName);
            _previewController.ShowWindow();
            Disposed = false;
        }

        public static void RegisterApplyCallback(Action<TriangleSelectorResult> callback)
        {
            OnApply += callback;
        }

        public static void Dispose()
        {
            if (!Disposed)
            {
                Disposed = true;
                _previewController.Dispose();
                OnApply = null;
            }
        }

        internal static void InvokeAndDispose(TriangleSelectorResult result)
        {
            if (!Disposed)
            {
                Disposed = true;
                _previewController.Dispose();
                OnApply?.Invoke(result);
                OnApply = null;
            }
        }

    }

}
