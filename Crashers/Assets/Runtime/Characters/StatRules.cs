using UnityEngine;

namespace Crashers.Characters
{
    /// <summary>
    /// Single source for derived character numbers. Every system calls these methods.
    /// </summary>
    public static class StatRules
    {
        /// <summary>Max health = baseHealth + defense * 4 + strength * 2.</summary>
        public static float MaxHealth(int baseHealth, int defense, int strength)
        {
            return baseHealth + defense * 4 + strength * 2;
        }

        /// <summary>Max mana = baseMana + magic * 5.</summary>
        public static float MaxMana(int baseMana, int magic)
        {
            return baseMana + magic * 5f;
        }

        /// <summary>Mana restored per second = 3.5 + magic * 0.25.</summary>
        public static float ManaRegen(int magic)
        {
            return 3.5f + magic * 0.25f;
        }

        /// <summary>Move speed = baseSpeed * (1 + agility * 0.03).</summary>
        public static float MoveSpeed(float baseSpeed, int agility)
        {
            return baseSpeed * (1f + agility * 0.03f);
        }

        /// <summary>Outgoing weapon damage = (8 + strength) * damageMultiplier.</summary>
        public static float OutgoingAttack(int strength, float multiplier)
        {
            return (8f + strength) * Mathf.Max(0f, multiplier);
        }

        /// <summary>Outgoing ability damage = baseDamage * (1 + magic * 0.08).</summary>
        public static float OutgoingAbility(int magic, float baseDamage)
        {
            return Mathf.Max(0f, baseDamage) * (1f + magic * 0.08f);
        }

        /// <summary>
        /// Damage after defense = outgoing * 100 / (100 + defense).
        /// A positive hit never mitigates below 1.
        /// </summary>
        public static float Mitigate(float outgoing, int defense)
        {
            if (outgoing <= 0f) return 0f;
            float mitigated = outgoing * (100f / (100f + Mathf.Max(0, defense)));
            return Mathf.Max(1f, mitigated);
        }
    }
}
