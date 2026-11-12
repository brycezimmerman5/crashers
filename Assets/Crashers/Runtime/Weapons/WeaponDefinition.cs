using Crashers.Combat;
using UnityEngine;

namespace Crashers.Weapons
{
    [CreateAssetMenu(menuName = "Crashers/Weapon Definition", fileName = "Weapon")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        public string Id = "weapon";
        public string DisplayName = "Weapon";
        public int StrengthMod;
        public int DefenseMod;
        public int MagicMod;
        public int AgilityMod;
        public AttackDefinition[] LightChain;
        public AttackDefinition[] HeavyChain;
        public AttackDefinition Aerial;
    }
}
