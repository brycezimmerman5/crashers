using Crashers.Content;
using UnityEditor;
using UnityEngine;

namespace Crashers.EditorTools
{
    public static class ContentMenu
    {
        const string Folder = "Assets/Crashers/Data";

        [MenuItem("Crashers/Export Runtime Definitions")]
        public static void Export()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Crashers"))
                AssetDatabase.CreateFolder("Assets", "Crashers");
            if (!AssetDatabase.IsValidFolder(Folder))
                AssetDatabase.CreateFolder("Assets/Crashers", "Data");

            GameContent content = GameContent.Create();
            AssetDatabase.StartAssetEditing();
            try
            {
                for (int i = 0; i < content.Assets.Count; i++)
                {
                    ScriptableObject asset = content.Assets[i];
                    if (asset == null || string.IsNullOrEmpty(asset.name)) continue;
                    string path = Folder + "/" + asset.name + ".asset";
                    if (AssetDatabase.LoadAssetAtPath<Object>(path) != null)
                        AssetDatabase.DeleteAsset(path);
                    AssetDatabase.CreateAsset(asset, path);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            content.Link();
            for (int i = 0; i < content.Assets.Count; i++)
            {
                if (content.Assets[i] != null) EditorUtility.SetDirty(content.Assets[i]);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Exported " + content.Assets.Count + " Crashers definitions to " + Folder + ".");
        }
    }
}
