using System.IO;
using UnityEditor;

namespace com.aoyon.triangleselector.utils
{
    public static class AssetUtility
    {
        public static void CreateDirectory(string folderpath)
        {
            if (!Directory.Exists(folderpath)) 
            {
                Directory.CreateDirectory(folderpath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        public static string GenerateVaildPath(string folderpath, string fileName, string fileExtension)
        {
            CreateDirectory(folderpath);
            string path = folderpath + "/" + fileName + "." + fileExtension;
            return AssetDatabase.GenerateUniqueAssetPath(path);
        }
    }
}