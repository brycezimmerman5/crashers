using Crashers.Characters;
using Crashers.Combat;
using Crashers.Magic;
using UnityEngine;

namespace Crashers.Enemies
{
    public enum EnemyRole
    {
        Melee = 0,
        Ranged = 1,
        Boss = 2
    }

    public enum PatternStep
    {
        Light = 0,
        Heavy = 1,
        Ability = 2
    }

    [CreateAssetMenu(menuName = "Crashers/Enemy Archetype", fileName = "Enemy")]
    public sealed class EnemyArchetype : ScriptableObject
    {
        public string Id = "enemy";
        public string DisplayName = "Enemy";
        public EnemyRole Role = EnemyRole.Melee;
        public CharacterDefinition Stats;
        public AttackDefinition[] LightChain;
        public AttackDefinition[] HeavyChain;
        public AttackDefinition Aerial;
        public AbilityDefinition RangedAbility;
        public float PreferredRange = 1.05f;
        public float RetreatHealthFraction = 0.25f;
        public int Xp = 30;
        public int Gold = 10;
        public string WeaponDropId;
        public float AggroRange = 10f;
        public PatternStep[] Pattern;
    }
}
