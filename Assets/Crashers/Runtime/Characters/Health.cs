using System;
using UnityEngine;

namespace Crashers.Characters
{
    public sealed class Health : MonoBehaviour
    {
        CharacterStats stats;
        float current;
        float max = 1f;

        public float Current => current;
        public float Max => max;
        public bool IsAlive { get; private set; } = true;
        public float Fraction => max <= 0.01f ? 0f : current / max;

        public event Action<Health, float> Damaged;
        public event Action<Health> Died;
        public event Action<Health> Revived;

        public void Bind(CharacterStats source)
        {
            stats = source;
            RefreshFromStats(false);
            current = max;
            IsAlive = true;
        }

        public void RefreshFromStats(bool healIncrease)
        {
            if (stats == null) return;
            float next = Mathf.Max(1f, stats.MaxHealth);
            float increase = next - max;
            max = next;
            if (healIncrease && increase > 0f && IsAlive) current += increase;
            if (current > max) current = max;
        }

        public void ApplyDamage(float outgoing)
        {
            if (!IsAlive) return;
            int defense = stats != null ? stats.Defense : 0;
            float amount = StatRules.Mitigate(outgoing, defense);
            current -= amount;
            Damaged?.Invoke(this, amount);
            if (current > 0f) return;
            current = 0f;
            IsAlive = false;
            Died?.Invoke(this);
        }

        public void Revive()
        {
            RefreshFromStats(false);
            current = max;
            IsAlive = true;
            Revived?.Invoke(this);
        }
    }
}
