using Crashers.Content;
using UnityEditor;
using UnityEngine;

namespace Crashers.EditorTools
{
    /// <summary>
    /// Loads the viking PSD and controller before Play so the arena can spawn them.
    /// </summary>
    [InitializeOnLoad]
    static class VikingArtLoader
    {
        const string PsdPath = "Assets/Art/vikingcrashers.psd";
        const string ControllerPath = "Assets/Art/vikingcrashers.controller";

        static VikingArtLoader()
        {
            EditorApplication.delayCall += Load;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void BeforeSceneLoad()
        {
            Load();
        }

        static void Load()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += Load;
                return;
            }

            VikingArt.Controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);
            VikingArt.Prefab = FindRig(PsdPath);
            VikingArt.Head = null;
            VikingArt.Torso = null;
            VikingArt.LeftHand = null;
            VikingArt.RightHand = null;
            VikingArt.LeftFoot = null;
            VikingArt.RightFoot = null;
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(PsdPath);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is not Sprite sprite) continue;
                switch (sprite.name.Trim().ToLowerInvariant())
                {
                    case "head":
                        VikingArt.Head = sprite;
                        break;
                    case "torso":
                        VikingArt.Torso = sprite;
                        break;
                    case "left hand":
                        VikingArt.LeftHand = sprite;
                        break;
                    case "right hand":
                        VikingArt.RightHand = sprite;
                        break;
                    case "left foot":
                        VikingArt.LeftFoot = sprite;
                        break;
                    case "right foot":
                        VikingArt.RightFoot = sprite;
                        break;
                }
            }
        }

        static GameObject FindRig(string path)
        {
            if (AssetDatabase.LoadMainAssetAtPath(path) is GameObject main && HasBone(main)) return main;
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is GameObject go && HasBone(go)) return go;
            }
            return null;
        }

        static bool HasBone(GameObject go)
        {
            if (go == null) return false;
            Transform[] transforms = go.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                if (transforms[i].name == "bone_6") return true;
            }
            return false;
        }
    }
}
