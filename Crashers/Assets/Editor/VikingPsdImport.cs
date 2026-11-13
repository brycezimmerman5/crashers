using System;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Crashers.EditorTools
{
    /// <summary>
    /// The viking PSD stores six layers. If Unity falls back to the texture importer,
    /// those layers collapse into one image. Crashers/Restore Viking PSD Slices puts
    /// the PSD Importer back and rebuilds the layer sprites.
    /// </summary>
    [InitializeOnLoad]
    static class VikingPsdImport
    {
        const string ArtPath = "Assets/Art/vikingcrashers.psd";
        const string SessionKey = "Crashers.VikingPsdSlices";
        const string RetryKey = "Crashers.VikingPsdRetries";

        static VikingPsdImport()
        {
            EditorApplication.delayCall += TryAutomaticRestore;
        }

        [MenuItem("Crashers/Restore Viking PSD Slices")]
        public static void RestoreFromMenu()
        {
            SessionState.EraseBool(SessionKey);
            string message = Restore();
            Debug.Log(message);
            EditorUtility.DisplayDialog("Viking PSD", message, "OK");
        }

        static void TryAutomaticRestore()
        {
            if (SessionState.GetBool(SessionKey, false)) return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += TryAutomaticRestore;
                return;
            }
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (FindPsdImporter() == null)
            {
                int retries = SessionState.GetInt(RetryKey, 0);
                if (retries > 40) return;
                SessionState.SetInt(RetryKey, retries + 1);
                EditorApplication.delayCall += TryAutomaticRestore;
                return;
            }

            string message = Restore();
            Debug.Log(message);
            SessionState.SetBool(SessionKey, true);
        }

        static string Restore()
        {
            if (AssetImporter.GetAtPath(ArtPath) == null)
            {
                return "Could not find " + ArtPath + ".\n\nOpen the Crashers folder as the Unity project, then run Crashers → Restore Viking PSD Slices again.";
            }

            Type importerType = FindPsdImporter();
            if (importerType == null)
            {
                return "The 2D PSD Importer package is not loaded yet.\n\nOpen Window → Package Manager and wait until 2D PSD Importer finishes installing. Then run Crashers → Restore Viking PSD Slices again.";
            }

            AssetImporter importer = AssetImporter.GetAtPath(ArtPath);
            if (importer.GetType() != importerType)
                SetImporterOverride(importerType);

            importer = AssetImporter.GetAtPath(ArtPath);
            if (importer == null || importer.GetType() != importerType)
            {
                return "Unity did not switch " + ArtPath + " to the PSD Importer. Select the file in the Project window, set the importer dropdown to PSD Importer, and run this menu again.";
            }

            var serialized = new SerializedObject(importer);
            SerializedProperty textureSettings = serialized.FindProperty("m_TextureImporterSettings");
            SetInt(textureSettings, "textureType", 8);
            SetInt(textureSettings, "spriteMode", 2);
            SetBool(serialized.FindProperty("m_MosaicLayers"), true);
            SetBool(serialized.FindProperty("m_CharacterMode"), true);
            SetBool(serialized.FindProperty("m_ResliceFromLayer"), true);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            importer.SaveAndReimport();

            importer = AssetImporter.GetAtPath(ArtPath);
            if (importer == null)
                return "The PSD disappeared during reimport.";

            serialized = new SerializedObject(importer);
            SetBool(serialized.FindProperty("m_ResliceFromLayer"), false);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            importer.SaveAndReimport();
            AssetDatabase.Refresh();

            string names = SpriteNames();
            if (string.IsNullOrEmpty(names))
            {
                return "The PSD Importer ran, but no layer sprites were created. Check the Console for an error that starts with \"Failed to import file\" on vikingcrashers.psd.";
            }

            return "Viking PSD slices are back. Expand Assets/Art/vikingcrashers in the Project window.\n\n" + names;
        }

        static string SpriteNames()
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(ArtPath);
            var builder = new StringBuilder();
            int count = 0;
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is not Sprite) continue;
                if (count > 0) builder.Append('\n');
                builder.Append(assets[i].name);
                count++;
            }
            return count == 0 ? string.Empty : builder.ToString();
        }

        static void SetInt(SerializedProperty parent, string name, int value)
        {
            if (parent == null) return;
            SerializedProperty property = parent.FindPropertyRelative(name);
            if (property != null) property.intValue = value;
        }

        static void SetBool(SerializedProperty property, bool value)
        {
            if (property != null) property.boolValue = value;
        }

        static Type FindPsdImporter()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                Type type = assemblies[i].GetType("UnityEditor.U2D.PSD.PSDImporter");
                if (type != null) return type;
            }
            return null;
        }

        static void SetImporterOverride(Type importerType)
        {
            MethodInfo[] methods = typeof(AssetDatabase).GetMethods(BindingFlags.Public | BindingFlags.Static);
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo method = methods[i];
                if (method.Name != "SetImporterOverride" || !method.IsGenericMethodDefinition) continue;
                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length != 1 || parameters[0].ParameterType != typeof(string)) continue;
                method.MakeGenericMethod(importerType).Invoke(null, new object[] { ArtPath });
                return;
            }
        }
    }
}
