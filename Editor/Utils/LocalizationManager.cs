using UnityEditor;
using System.Collections.Generic;
using System;

namespace com.aoyon.triangleselector.utils
{
    public static class LocalizationManager
    {
        public static string[] availableLanguages = new[] { "English", "日本語" };

        public static int GetSelectedLanguageIndex(string preferenceKey)
        {
            return Array.IndexOf(availableLanguages, EditorPrefs.GetString(preferenceKey, availableLanguages[0]));
        }

        public static string GetLocalizedText(Dictionary<string, string[]> localizedText, string key, int selectedLanguageIndex)
        {
            if (localizedText.ContainsKey(key))
            {
                return localizedText[key][selectedLanguageIndex];
            }

            return $"[Missing: {key}]";
        }

        public static void SetLanguage(int languageIndex, string preferenceKey)
        {
            EditorPrefs.SetString(preferenceKey, availableLanguages[languageIndex]);
        }

        public static void RenderLocalize(ref int selectedLanguageIndex, string preferenceKey)
        {
            int languageIndex = EditorGUILayout.Popup("Language", selectedLanguageIndex, availableLanguages);
            if (languageIndex != selectedLanguageIndex)
            {
                selectedLanguageIndex = languageIndex;
                SetLanguage(languageIndex, preferenceKey);
            }
        }
    }
}