using UnityEngine;

namespace Crashers.Progression
{
    [CreateAssetMenu(menuName = "Crashers/Level Curve", fileName = "LevelCurve")]
    public sealed class LevelCurve : ScriptableObject
    {
        public int BaseXp = 40;
        public int XpPerLevel = 30;

        public int RequiredForNext(int currentLevel)
        {
            int level = currentLevel < 1 ? 1 : currentLevel;
            return Mathf.Max(1, BaseXp + (level - 1) * XpPerLevel);
        }
    }
}
