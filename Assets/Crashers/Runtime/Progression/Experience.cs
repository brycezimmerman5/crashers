using UnityEngine;

namespace Crashers.Progression
{
    public sealed class Experience : MonoBehaviour
    {
        public int Level { get; private set; } = 1;
        public int Xp { get; private set; }

        public void Set(int level, int xp)
        {
            Level = level < 1 ? 1 : level;
            Xp = xp < 0 ? 0 : xp;
        }

        public void AddXp(int amount)
        {
            if (amount > 0) Xp += amount;
        }

        public void SpendXp(int amount)
        {
            Xp -= amount;
            if (Xp < 0) Xp = 0;
        }

        public void SetLevel(int level) => Level = level < 1 ? 1 : level;
    }
}
