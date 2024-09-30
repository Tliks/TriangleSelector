using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using com.aoyon.triangleselector.utils;

namespace com.aoyon.triangleselector
{


    public class RenderSelector : Editor
    {
        private SkinnedMeshRenderer _skinnedMeshRenderer;
        private Mesh _mesh;
        private TriangleSelectionContainer _triangleSelectionContainer;
        private List<TriangleSelection> _triangleSelections;
        private Action<List<int>> _onSelectionChanged;
        private string[] _displayedOptions;
        private int _selectedIndex = 0;
        private string _noneLabel;


        public void Initialize(SkinnedMeshRenderer skinnedMeshRenderer, IReadOnlyList<int> defaultSelection, string label = "None")
        {
            _skinnedMeshRenderer = skinnedMeshRenderer;
            _mesh = _skinnedMeshRenderer.sharedMesh;
            _noneLabel = label;
            LoadAsset();
            _selectedIndex = SaveAsScriptableObject.FindIndex(_triangleSelections, defaultSelection);
        }

        public void RegisterApplyCallback(Action<List<int>> onSelectionChanged)
        {
            _onSelectionChanged = onSelectionChanged;
        }

        public void Dispose()
        {
            if (!TriangleSelector.Disposed)
            {
                TriangleSelector.Dispose();
            }
        }

        public void RenderGUI()
        {
            LocalizationEditor.RenderLocalize();

            GUI.enabled = TriangleSelector.Disposed;
            using (new GUILayout.HorizontalScope())
            {   
                GUILayout.Label(LocalizationEditor.GetLocalizedText("TriangleSelection.TriangleSelection"));
                RenderTriangleSelection();

                RenderRemoveSelection();
            }
            GUI.enabled = true;

            RenderEditSelection();
        }


        public void RenderTriangleSelection(GUILayoutOption[] options = null)
        {
            int selectedIndex = EditorGUILayout.Popup(_selectedIndex, _displayedOptions, options);
            if (selectedIndex != _selectedIndex)
            {   
                _selectedIndex = selectedIndex;
                CallSelectionChange(_triangleSelections[_selectedIndex].selection);
            }
        }

        public void RenderRemoveSelection()
        {
            GUI.enabled = _selectedIndex != 0;
            if (GUILayout.Button(LocalizationEditor.GetLocalizedText("TriangleSelection.Remove")))
            {
                SaveAsScriptableObject.RemoveData(_triangleSelectionContainer, _selectedIndex);
                LoadAsset();
                _selectedIndex = Math.Max(0, _selectedIndex - 1);
                CallSelectionChange(_triangleSelections[_selectedIndex].selection);
            }
            GUI.enabled = true;
        }

        public void RenderEditSelection(GUILayoutOption[] options = null)
        {
            string add = LocalizationEditor.GetLocalizedText("TriangleSelection.Add");
            string edit = LocalizationEditor.GetLocalizedText("TriangleSelection.Edit");
            string close = LocalizationEditor.GetLocalizedText("TriangleSelection.CloseSelector");
            string[] labels = new string[] { add, edit, close };
            RenderEditSelection(labels, options);
        }

        public void RenderEditSelection(string[] labels, GUILayoutOption[] options = null)
        {
            string label;

            // OpenSelector
            if (TriangleSelector.Disposed)
            {
                label = labels[_selectedIndex == 0 ? 0 : 1];
                if (GUILayout.Button(label, options))
                {
                    /// Add
                    if (_selectedIndex == 0)
                    {
                        TriangleSelector.Initialize(_skinnedMeshRenderer);
                    }
                    // Edit
                    else
                    {
                        var defaultSelection = _triangleSelections[_selectedIndex];
                        TriangleSelector.Initialize(_skinnedMeshRenderer, defaultSelection.selection, defaultSelection.displayname);
                    }
                    TriangleSelector.RegisterApplyCallback(ProcessNewSelection);
                }
            }
            // CloseSelector
            else
            {
                label = labels[2];
                if (GUILayout.Button(label, options))
                {
                    TriangleSelector.Dispose();
                }
            }
        }

        private void ProcessNewSelection(TriangleSelectorResult result)
        {
            List<int> newSelection = result.SelectedTriangleIndices;
            if (newSelection != null && newSelection.Count > 0)
            {
                string displayname = result.SelectionName;
                // displaynameが未入力時は自動決定
                if (displayname == null || displayname == "")
                {
                    string uniquename = SaveAsScriptableObject.GetUniqueDisplayname(_triangleSelections);
                    displayname = uniquename;
                }

                if (result.SaveMode == SaveModes.New || result.SaveMode == SaveModes.EditNew)
                {
                    TriangleSelection newTriangleSelection = new TriangleSelection { selection = newSelection };
                    newTriangleSelection.displayname = displayname;
                    newTriangleSelection.createtime = SaveAsScriptableObject.GetTimestamp();
                    SaveAsScriptableObject.AddData(_triangleSelectionContainer, newTriangleSelection);
                }
                else if (result.SaveMode == SaveModes.OverWrite)
                {
                    TriangleSelection currentTriangleSelection = _triangleSelectionContainer.selections[_selectedIndex];
                    currentTriangleSelection.selection = newSelection;
                    currentTriangleSelection.displayname = displayname;
                    currentTriangleSelection.createtime = SaveAsScriptableObject.GetTimestamp();
                    SaveAsScriptableObject.UpdateData(_triangleSelectionContainer);
                }
                else
                {
                    throw new InvalidCastException("invaild SaveMode");
                }
 
                LoadAsset();
                _selectedIndex = SaveAsScriptableObject.FindIndex(_triangleSelections, newSelection);
                CallSelectionChange(_triangleSelections[_selectedIndex].selection);
            }  
        }

        private void LoadAsset()
        {
            _triangleSelectionContainer = SaveAsScriptableObject.GetContainer(_mesh);
            SaveAsScriptableObject.AddDefaultSelection(_triangleSelectionContainer);
            _triangleSelections = _triangleSelectionContainer.selections;
            _displayedOptions = _triangleSelections
                .Select(ts => ts.selection.Count() == 0 
                    ? _noneLabel 
                    : $"{ts.displayname} ({SaveAsScriptableObject.CalculatePercent(ts.selection.Count(), _triangleSelectionContainer.TriangleCount)}%)")
                .ToArray();
        }

        private void CallSelectionChange(List<int> newValues)
        {
            _onSelectionChanged?.Invoke(new List<int>(newValues));
        }
    }

    public class SaveAsScriptableObject
    {
        private const string SAVE_PATH = "Assets/TriangleSelector/TriangleSelection";

        public static void AddData(TriangleSelectionContainer triangleSelection, TriangleSelection newSelection)
        {   
            int index = FindIndex(triangleSelection.selections, newSelection);
            if (index != -1)
            {
                RemoveData(triangleSelection, index);
            }
            triangleSelection.selections.Add(newSelection);
            UpdateData(triangleSelection);
        }

        public static void RemoveData(TriangleSelectionContainer triangleSelection, int index)
        {
            if (index >= 0 && index < triangleSelection.selections.Count)
            {
                triangleSelection.selections.RemoveAt(index);
                UpdateData(triangleSelection);
            }
        }

        public static void UpdateData(TriangleSelectionContainer container)
        {
            EditorUtility.SetDirty(container);
            AssetDatabase.SaveAssets();
        }

        public static int FindIndex(List<TriangleSelection> triangleSelections, TriangleSelection triangleSelection)
        {
            int index = FindIndex(triangleSelections, triangleSelection.selection);
            return index;
        }

        public static int FindIndex(List<TriangleSelection> triangleSelections, IReadOnlyList<int> selection)
        {
            int index = triangleSelections.FindIndex(ts => ts.selection.SequenceEqual(selection));
            return index;
        }

        public static int FindIndex(List<TriangleSelection> triangleSelections, SerializedProperty listProperty)
        {
            int index = FindIndex(triangleSelections, ConvertToList(listProperty));
            return index;
        }

        private static List<int> ConvertToList(SerializedProperty listProperty)
        {
            List<int> intList = new List<int>();
            for (int i = 0; i < listProperty.arraySize; i++)
            {
                intList.Add(listProperty.GetArrayElementAtIndex(i).intValue);
            }
            return intList;
        }

        public static TriangleSelectionContainer GetContainer(Mesh mesh)
        {
            AssetUtility.CreateDirectory(SAVE_PATH);
            string[] guids = AssetDatabase.FindAssets("t:TriangleSelectionContainer", new[] { SAVE_PATH });
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                TriangleSelectionContainer selection = AssetDatabase.LoadAssetAtPath<TriangleSelectionContainer>(assetPath);
                if (selection != null && selection.mesh == mesh)
                {
                    int targetcount = mesh.triangles.Count() / 3;
                    int cachecount = selection.TriangleCount != 0 ? selection.TriangleCount : targetcount; // TriangleCountが0の場合はtargetcountを使用
                    if (targetcount != cachecount)
                    {
                        Debug.LogWarning($"Mesh changes detected. A new TriangleSelectionContainer will be created. Current polygon count: {targetcount}. Saved polygon count: {cachecount}.");
                    }
                    else
                    {
                        return selection;
                    }
                }
            }

            //Debug.Log("存在しない場合は新規作成");
            return CreateAsset(mesh);
        }

        private static TriangleSelectionContainer CreateAsset(Mesh mesh)
        {
            var instance = ScriptableObject.CreateInstance<TriangleSelectionContainer>();
            instance.mesh = mesh;
            instance.TriangleCount = mesh.triangles.Count() / 3;

            if (!Directory.Exists(SAVE_PATH)) Directory.CreateDirectory(SAVE_PATH);
            string uniquePath = AssetUtility.GenerateVaildPath(SAVE_PATH, mesh.name,  "asset");
            AssetDatabase.CreateAsset(instance, uniquePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var loadedInstance = (TriangleSelectionContainer)AssetDatabase.LoadAssetAtPath(uniquePath, typeof(TriangleSelectionContainer));

            AddDefaultSelection(loadedInstance);
            return loadedInstance;
        }

        public static void AddDefaultSelection(TriangleSelectionContainer container)
        {
            bool hasEmptySelection = container.selections.Any(s => s.selection.Count == 0);
            if (!hasEmptySelection)
            {
                var defaultSelection = new TriangleSelection() { selection = new List<int>() };
                container.selections.Insert(0, defaultSelection);
                UpdateData(container);
            }
        }

        public static string GetUniqueDisplayname(List<TriangleSelection> selections)
        {
            int maxIndex = -1;
            var regex = new Regex(@"Selection(\d+)");

            foreach (var selection in selections)
            {
                var match = regex.Match(selection.displayname);
                if (match.Success && int.TryParse(match.Groups[1].Value, out int index))
                {
                    if (index > maxIndex)
                    {
                        maxIndex = index;
                    }
                }
            }

            return $"Selection{maxIndex + 1}";
        }

        public static long GetTimestamp()
        {
            DateTime now = DateTime.Now;

            int year = now.Year % 100;
            int month = now.Month;
            int day = now.Day;
            int hour = now.Hour;
            int minute = now.Minute;
            int second = now.Second;

            string timestampStr = $"{year:D2}{month:D2}{day:D2}{hour:D2}{minute:D2}{second:D2}";
            long timestamp = long.Parse(timestampStr);

            return timestamp;
        }

        public static int CalculatePercent(int selection, int total)
        {
            float percent = (float)selection / (float)total * 100;
            percent = (int)Math.Round((double)percent);
            return (int)percent;
        }

    }
}
