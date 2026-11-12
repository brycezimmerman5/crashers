using System;
using UnityEngine;

namespace Crashers.Characters
{
    public sealed class CharacterStats : MonoBehaviour
    {
        CharacterDefinition definition;
        int levelBonus;
        int weaponStrength;
        int weaponDefense;
        int weaponMagic;
        int weaponAgility;

        public CharacterDefinition Definition => definition;
        public int Strength => Safe(definition != null ? definition.Strength : 1) + levelBonus + weaponStrength;
        public int Defense => Safe(definition != null ? definition.Defense : 0) + levelBonus + weaponDefense;
        public int Magic => Safe(definition != null ? definition.Magic : 1) + levelBonus + weaponMagic;
        public int Agility => Safe(definition != null ? definition.Agility : 1) + levelBonus + weaponAgility;
        public float MaxHealth => StatRules.MaxHealth(definition != null ? definition.BaseHealth : 40, Defense, Strength);
        public float MaxMana => StatRules.MaxMana(definition != null ? definition.BaseMana : 10, Magic);
        public float MoveSpeed => StatRules.MoveSpeed(definition != null ? definition.BaseMoveSpeed : 4f, Agility);
        public float ManaRegen => StatRules.ManaRegen(Magic);

        public event Action Changed;

        public void Configure(CharacterDefinition source)
        {
            definition = source;
            levelBonus = 0;
            weaponStrength = 0;
            weaponDefense = 0;
            weaponMagic = 0;
            weaponAgility = 0;
            Changed?.Invoke();
        }

        public void SetLevelBonus(int bonusPerStat)
        {
            levelBonus = Mathf.Max(0, bonusPerStat);
            Changed?.Invoke();
        }

        public void SetWeaponMods(int strength, int defense, int magic, int agility)
        {
            weaponStrength = strength;
            weaponDefense = defense;
            weaponMagic = magic;
            weaponAgility = agility;
            Changed?.Invoke();
        }

        static int Safe(int value) => value < 0 ? 0 : value;
    }
}
