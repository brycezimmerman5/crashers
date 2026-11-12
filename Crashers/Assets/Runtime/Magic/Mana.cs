using Crashers.Characters;
using UnityEngine;

namespace Crashers.Magic
{
    public sealed class Mana : MonoBehaviour
    {
        CharacterStats stats;
        float current;
        float max = 1f;

        public float Current => current;
        public float Max => max;

        public void Bind(CharacterStats source)
        {
            stats = source;
            max = Mathf.Max(1f, source != null ? source.MaxMana : 1f);
            current = max;
        }

        public void RefreshFromStats(bool grantIncrease)
        {
            if (stats == null) return;
            float next = Mathf.Max(1f, stats.MaxMana);
            float increase = next - max;
            max = next;
            if (grantIncrease) current += Mathf.Max(0f, increase);
            current = Mathf.Clamp(current, 0f, max);
        }

        public bool TrySpend(float cost)
        {
            if (cost <= 0f) return true;
            if (current < cost) return false;
            current -= cost;
            return true;
        }

        void Update()
        {
            if (stats == null) return;
            float regen = stats.ManaRegen;
            current = Mathf.Min(max, current + regen * Time.deltaTime);
        }
    }
}
