using UnityEngine;

namespace Crashers.Progression
{
    public sealed class Wallet : MonoBehaviour
    {
        public int Gold { get; private set; }

        public void Set(int gold) => Gold = gold < 0 ? 0 : gold;

        public void Add(int amount)
        {
            if (amount > 0) Gold += amount;
        }
    }
}
