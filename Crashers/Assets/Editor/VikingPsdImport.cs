using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Crashers.EditorTools
{
    /// <summary>
    /// The viking PSD stores six layers. If Unity falls back to the texture importer,
    /// those layers collapse into one image. This puts the PSD Importer back and
    /// rebuilds the layer sprites.
    /// </summary>
    [InitializeOnLoad]
    static class VikingPsdImport
    {
        const string ArtPath = "Assets/Art/vikingcrashers.psd";
        const string SessionKey = "Crashers.VikingPsdSlices";

        static VikingPsdImport()
        {
            EditorApplication.delayCall += EnsureSlicedImport;
        }

        static void EnsureSlicedImport()
        {
            if (SessionState.GetBool(SessionKey, false)) return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += EnsureSlicedImport;
                return;
            }
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (AssetImporter.GetAtPath(ArtPath) == null) return;

            Type importerType = FindPsdImporter();
            if (importerType == null) return;

            AssetImporter importer = AssetImporter.GetAtPath(ArtPath);
            if (importer.GetType() != importerType)
                SetImporterOverride(importerType);

            importer = AssetImporter.GetAtPath(ArtPath);
            if (importer == null || importer.GetType() != importerType) return;

            if (SpriteCount() >= 6)
            {
                SessionState.SetBool(SessionKey, true);
                return;
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
            if (importer == null) return;
            serialized = new SerializedObject(importer);
            SetBool(serialized.FindProperty("m_ResliceFromLayer"), false);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            importer.SaveAndReimport();
            SessionState.SetBool(SessionKey, true);
        }

        static int SpriteCount()
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(ArtPath);
            int count = 0;
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Sprite) count++;
            }
            return count;
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
