using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Crashers.EditorTools
{
    [InitializeOnLoad]
    static class ArenaAutoOpen
    {
        const string ArenaPath = "Assets/Crashers/Scenes/Arena.unity";

        static ArenaAutoOpen()
        {
            EditorApplication.delayCall += OpenArenaIfEmpty;
        }

        static void OpenArenaIfEmpty()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += OpenArenaIfEmpty;
                return;
            }
            if (SessionState.GetBool("Crashers.OpenedArena", false)) return;
            SessionState.SetBool("Crashers.OpenedArena", true);
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            Scene active = SceneManager.GetActiveScene();
            if (!string.IsNullOrEmpty(active.path)) return;
            EditorSceneManager.OpenScene(ArenaPath);
        }
    }
}
