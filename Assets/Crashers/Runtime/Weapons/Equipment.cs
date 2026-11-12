using Crashers.Characters;
using Crashers.Combat;
using UnityEngine;

namespace Crashers.Weapons
{
    public sealed class Equipment : MonoBehaviour
    {
        Inventory inventory;
        CharacterStats stats;
        CombatController combat;

        public WeaponDefinition Equipped { get; private set; }

        public void Configure(Inventory bag, CharacterStats body, CombatController attacks)
        {
            inventory = bag;
            stats = body;
            combat = attacks;
        }

        public void Equip(WeaponDefinition weapon)
        {
            if (weapon == null) return;
            if (inventory != null) inventory.Add(weapon);
            Equipped = weapon;
            if (stats != null)
                stats.SetWeaponMods(weapon.StrengthMod, weapon.DefenseMod, weapon.MagicMod, weapon.AgilityMod);
            if (combat != null)
                combat.SetLoadout(weapon.LightChain, weapon.HeavyChain, weapon.Aerial);
        }
    }
}
