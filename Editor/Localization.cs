using System.Collections.Generic;
using com.aoyon.triangleselector.utils;

namespace com.aoyon.triangleselector
{
    public static class LocalizationEditor
    {
        private static Dictionary<string, string[]> _LocalizedText = new Dictionary<string, string[]>
        {
            {"TriangleSelection.TriangleSelection", new string[] {"Selection", "選択箇所"} },
            {"TriangleSelection.Remove", new string[] {"Remove", "削除"} },
            {"TriangleSelection.Add", new string[] {"Add New Selection", "新しい選択箇所を追加"} },
            {"TriangleSelection.Edit", new string[] {"Edit Current Selection", "現在の選択箇所を編集"} },
            {"TriangleSelection.CloseSelector", new string[] {"Close Triangle Selector", "Triangle Selectorを閉じる"} },
            {"TriangleSelection.EnablePreview", new string[] {"Enable NDMF Preview", "NDMFプレビューを有効化"} },
            {"TriangleSelection.DisablePreview", new string[] {"Disable NDMF Preview", "NDMFプレビューを無効化"} },

            { "TriangleSelector.SelectedTotalPolygonsLabel", new string[] { "Selected/Total Polygons", "選択中/全ポリゴン" } },
            { "TriangleSelector.commondescription", new string[] {"You can select parts of the mesh by clicking on the scene. You can also select a range by dragging.", "シーン上でクリックすることでメッシュの一部を選択できます。ドラッグすることで範囲選択も可能です。"}},
            { "TriangleSelector.SelectAllButton", new string[] { "Select All", "すべて選択" } },
            { "TriangleSelector.UnselectAllButton", new string[] { "Unselect All", "すべて解除" } },
            { "TriangleSelector.ReverseAllButton", new string[] { "Reverse All", "すべて反転" } },
            { "TriangleSelector.UndoButton", new string[] { "Undo (Ctrl+Z)", "元に戻す (Ctrl+Z)" } },
            { "TriangleSelector.RedoButton", new string[] { "Redo (Ctrl+Y)", "やり直す (Ctrl+Y)" } },
            { "TriangleSelector.EnableSelectionButton", new string[] { "Resume Selection", "選択を再開" } },
            { "TriangleSelector.DisableSelectionButton", new string[] { "Stop Selection", "選択を停止" } },
            { "TriangleSelector.SelectionMode", new string[] { "SelectMode", "選択モード" }},

            { "TriangleSelector.islandMode", new string[] { "Island", "アイランド" }},
            { "TriangleSelector.island.description", new string[] {"In Island Mode, you can select structurally separated parts of the mesh (islands). For range selection, drag to ensure the desired  island is completely within the selection range.", "アイランドモードでは構造的に分離されたメッシュの一部(アイランド)が選択できます。範囲選択では選択したいアイランドが範囲内に完全に入るようにドラックしてください。"}},
            { "TriangleSelector.island.SplitMeshMoreToggle", new string[] { "Split mesh more", "メッシュをさらに分割" } },
            { "TriangleSelector.island.SplitMeshMoreToggle.description", new string[] {" Split the mesh into more detailed islands.", "メッシュをさらに多くのアイランドに細かく分離します。"}},
            { "TriangleSelector.island.SelectAllInRangeToggle", new string[] { "Select all in range", "範囲内をすべて選択" } },
            { "TriangleSelector.island.SelectAllInRangeToggle.description", new string[] {"Range selection options. Change this to select island that are even partially within the range.", "範囲選択に関するオプションです。一部でも範囲内にあるアイランドも選択されるように変更します。"}},
            
            { "TriangleSelector.polygonMode", new string[] { "Polygon", "ポリゴン" }},
            { "TriangleSelector.polygon.description", new string[] {"In Polygon Mode, you can directly select polygons.", "ポリゴンモードではポリゴンを直接選択できます。"}},
            { "TriangleSelector.polygon.scale", new string[] { "Scale", "スケール" }},
            { "TriangleSelector.polygon.scale.description", new string[] { "The value that determines how far polygons from the clicked point are included in the selection.", "クリックした箇所からどれだけ離れた位置にあるポリゴンまで選択対象に含めるかの値です。" }},

            { "TriangleSelector.SelectionName", new string[] { "SaveName(Optional)", "保存名(オプション)"}},
            { "TriangleSelector.SaveMode", new string[] { "Save Method", "保存方法"}},
            { "TriangleSelector.SaveMode.overwrite", new string[] { "Overwrite", "上書き"}},
            { "TriangleSelector.SaveMode.EditNew", new string[] { "New", "新規"}},
            { "TriangleSelector.Apply", new string[] { "Apply", "適用"}}
            
        };
        
        private const string PreferenceKey = "com.aoyon.triangleselector.lang";
        private static int selectedLanguageIndex = LocalizationManager.GetSelectedLanguageIndex(PreferenceKey);

        public static string GetLocalizedText(string key)
        {
            return LocalizationManager.GetLocalizedText(_LocalizedText, key, selectedLanguageIndex);
        }

        public static void RenderLocalize()
        {
            LocalizationManager.RenderLocalize(ref selectedLanguageIndex, PreferenceKey);
        }

    }

}