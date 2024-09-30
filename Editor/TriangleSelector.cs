using System;
using System.Collections.Generic;
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
        

        public static void Initialize(SkinnedMeshRenderer skinnedMeshRenderer)
        {
            Initialize(skinnedMeshRenderer, new List<int>(), null);
        }

        public static void Initialize(SkinnedMeshRenderer skinnedMeshRenderer, IReadOnlyCollection<int> defaultTriangleIndices)
        {
            Initialize(skinnedMeshRenderer, defaultTriangleIndices, null);
        }

        public static void Initialize(SkinnedMeshRenderer skinnedMeshRenderer, string defaultSelectionName)
        {
            Initialize(skinnedMeshRenderer, new List<int>(), defaultSelectionName);
        }

        public static void Initialize(SkinnedMeshRenderer skinnedMeshRenderer, IReadOnlyCollection<int> defaultTriangleIndices, string defaultSelectionName)
        {
            _previewController = new PreviewController();
            _previewController.Initialize(skinnedMeshRenderer, defaultTriangleIndices, defaultSelectionName);
            _previewController.ShowWindow();
            Disposed = false;
        }

        public static void RegisterApplyCallback(Action<TriangleSelectorResult> callback)
        {
            OnApply += callback;
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

        internal static void Dispose()
        {
            if (!Disposed)
            {
                Disposed = true;
                _previewController.Dispose();
                OnApply = null;
            }
        }

    }

}
