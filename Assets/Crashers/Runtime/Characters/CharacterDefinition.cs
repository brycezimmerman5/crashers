using Crashers.Combat;
using UnityEngine;

namespace Crashers.Characters
{
    [CreateAssetMenu(menuName = "Crashers/Character Definition", fileName = "Character")]
    public sealed class CharacterDefinition : ScriptableObject
    {
        public string Id = "character";
        public string DisplayName = "Character";
        public int Strength = 5;
        public int Defense = 5;
        public int Magic = 5;
        public int Agility = 5;
        public int BaseHealth = 80;
        public int BaseMana = 20;
        public float BaseMoveSpeed = 5.2f;
        public AttackDefinition[] LightChain;
        public AttackDefinition[] HeavyChain;
        public AttackDefinition Aerial;
    }
}
