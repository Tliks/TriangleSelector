using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Overlays;
using static com.aoyon.triangleselector.LocalizationEditor;

namespace com.aoyon.triangleselector
{
    public enum SelectionModes
    {
        Island,
        Polygon
    }

    public enum SaveModes
    {
        // 新規選択
        New = -1,
        // 編集モード(新規保存)
        EditNew = 0,
        // 編集モード(上書き保存)
        OverWrite = 1
    }

    public class TriangleSelectorOptions
    {
        public SelectionModes SelectionMode = SelectionModes.Island;
        public bool MergeSamePosition = true;
        public bool CheckAll = true;
        public float Scale = 0.03f;

        public SaveModes SaveMode;
        public string SelectionName;
    }

    public class TriangleSelectorOverlay : IMGUIOverlay
    {
        public const string DISPLAY_NAME = "TriangleSelector";
        private static readonly Vector2 NEW_MODE_SIZE = new Vector2(255f, 255f);
        private static readonly Vector2 EDIT_MODE_SIZE = new Vector2(255f, 275f);
        private static readonly Vector2 MAX_SIZE = new Vector2(1000f, 1000f);

        public static TriangleSelectorOptions Options;
        private static PreviewController _previewController;

        private static TriangleSelectorOverlay _overlay;
        private static Vector2 _size = NEW_MODE_SIZE;

        public static void Initialize(PreviewController previewController, string selectionName)
        {
            _previewController = previewController;
            Options = new();

            // NewMode
            if (selectionName == null || selectionName == "")
            {
                Options.SaveMode = SaveModes.New;
                Options.SelectionName = "";
                _size = NEW_MODE_SIZE;

            }
            // EditMode
            else
            {
                Options.SaveMode = SaveModes.OverWrite;
                Options.SelectionName = selectionName;
                _size = EDIT_MODE_SIZE;
            }

            _overlay?.Close();

        }

        public static void ShowOverlay(SceneView sceneView)
        {
            _overlay = new TriangleSelectorOverlay();
            //_overlay.displayName = DISPLAY_NAME;
            sceneView.overlayCanvas.Add(_overlay);
            _overlay.displayed = true;
            _overlay.maxSize = MAX_SIZE;
            _overlay.size = _size;
            _overlay.floatingPosition = new Vector2(
                sceneView.position.width - _size.x, 
                sceneView.position.height - _size.y
            );
        }

        public override void OnGUI()
        {
            // localize
            LocalizationEditor.RenderLocalize();
            
            EditorGUILayout.Space();

            // labels
            string label  = $"{GetLocalizedText("TriangleSelector.SelectedTotalPolygonsLabel")}: {_previewController.UnselectedMeshRenderer.name}";
            GUILayout.Label(label, EditorStyles.boldLabel);
            GUILayout.Label($"{_previewController.TriangleSelectionManager.GetSelectedTriangles().Count}/{_previewController.TriangleSelectionManager.GetAllTriangles().Count}");
            //EditorGUILayout.HelpBox(GetLocalizedText("TriangleSelector.commondescription"), MessageType.Info);

            EditorGUILayout.Space();

            // SelectAll etc
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button(GetLocalizedText("TriangleSelector.SelectAllButton")))
                {
                    _previewController.SelectAll();
                }

                if (GUILayout.Button(GetLocalizedText("TriangleSelector.UnselectAllButton")))
                {
                    _previewController.UnselectAll();
                }

                if (GUILayout.Button(GetLocalizedText("TriangleSelector.ReverseAllButton")))
                {
                    _previewController.ReverseAll();
                }
            }
        
            // Undo/Redo
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button(GetLocalizedText("TriangleSelector.UndoButton")))
                {
                    _previewController.PerformUndo();
                }

                if (GUILayout.Button(GetLocalizedText("TriangleSelector.RedoButton")))
                {
                    _previewController.PerformRedo();
                }

            }

            EditorGUILayout.Space();

            // SelectionMode
            using (new GUILayout.HorizontalScope())
            {
                GUILayoutOption[] options = {GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false)};
                GUILayout.Label(GetLocalizedText("TriangleSelector.SelectionMode"), options);

                string[] modes = { GetLocalizedText("TriangleSelector.islandMode"), GetLocalizedText("TriangleSelector.polygonMode") };
                Options.SelectionMode = (SelectionModes)GUILayout.Toolbar((int)Options.SelectionMode, modes);
                
                string lebal;
                if (Options.SelectionMode == SelectionModes.Island)
                {
                    lebal = GetLocalizedText("TriangleSelector.island.description");
                }
                else if (Options.SelectionMode == SelectionModes.Polygon)
                {
                    lebal = GetLocalizedText("TriangleSelector.polygon.description");
                }
                else
                {
                    throw new InvalidOperationException("invalid selection mode");
                }
                RenderInfo(lebal);
            }


            EditorGUILayout.Space();

            // options
            if (Options.SelectionMode == SelectionModes.Island)
            {
                using (new GUILayout.HorizontalScope())
                {
                    string splitLabel = GetLocalizedText("TriangleSelector.island.SplitMeshMoreToggle");
                    Options.MergeSamePosition = !EditorGUILayout.Toggle(splitLabel, !Options.MergeSamePosition);
                    RenderInfo(GetLocalizedText("TriangleSelector.island.SplitMeshMoreToggle.description"));
                }

                using (new GUILayout.HorizontalScope())
                {
                    string selectAlllabel = GetLocalizedText("TriangleSelector.island.SelectAllInRangeToggle");
                    Options.CheckAll = !EditorGUILayout.Toggle(selectAlllabel, !Options.CheckAll);
                    RenderInfo(GetLocalizedText("TriangleSelector.island.SelectAllInRangeToggle.description"));
                }
            }
            else if (Options.SelectionMode == SelectionModes.Polygon)
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label(GetLocalizedText("TriangleSelector.polygon.scale"));
                    Options.Scale = EditorGUILayout.Slider(Options.Scale, 0.0f, 0.1f);
                    RenderInfo(GetLocalizedText("TriangleSelector.polygon.scale.description"));  
                }
            }
            else
            {
                throw new InvalidOperationException("invalid selection mode");
            }

            EditorGUILayout.Space();

            // savename
            Options.SelectionName = EditorGUILayout.TextField(GetLocalizedText("TriangleSelector.SelectionName"), Options.SelectionName);

            // savemode
            if (Options.SaveMode != SaveModes.New)
            {
                string modelabel = GetLocalizedText("TriangleSelector.SaveMode");
                
                int selectedIndex = (int)Options.SaveMode;

                string editnewlabel = GetLocalizedText("TriangleSelector.SaveMode.EditNew");
                string overwritelabel = GetLocalizedText("TriangleSelector.SaveMode.overwrite");
                var modeslavel = new string[] { editnewlabel, overwritelabel};

                Options.SaveMode = (SaveModes)EditorGUILayout.Popup(modelabel, selectedIndex, modeslavel);
            }

            // apply
            GUI.enabled = _previewController.TriangleSelectionManager.GetSelectedTriangles().Count > 0;
            if (GUILayout.Button(GetLocalizedText("TriangleSelector.Apply")))
            {
                _previewController.Apply();
            }
            GUI.enabled = true;

        }

        private void RenderInfo(string label)
        {
            GUIContent infoContent = new GUIContent(EditorGUIUtility.IconContent("_Help"));
            infoContent.tooltip = label;
            GUILayoutOption[] options = {GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false)};
            GUILayout.Label(infoContent, options);
        }
    
    }
}
    