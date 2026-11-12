using Crashers.Characters;
using Crashers.Combat;
using Crashers.Content;
using Crashers.Magic;
using UnityEngine;

namespace Crashers.Weapons
{
    public sealed class Equipment : MonoBehaviour
    {
        static readonly Color DefaultTint = new Color(1f, 0.86f, 0.25f);

        enum Shown
        {
            Fists,
            Weapon
        }

        Inventory inventory;
        CharacterStats stats;
        CombatController combat;
        HeldWeaponView held;
        HeldFistsView fists;
        Health health;
        Shown shown = Shown.Fists;
        Sprite weaponArt;
        Color weaponTint;
        float weaponScale = 1f;

        public WeaponDefinition Equipped { get; private set; }

        public void Configure(Inventory bag, CharacterStats body, CombatController attacks)
        {
            inventory = bag;
            stats = body;
            combat = attacks;
            EnsureHeld();
            EnsureFists();
            if (Equipped == null) PresentFists();
            else if (fists != null) fists.Hide();
            var life = GetComponent<Health>();
            if (life != null && life != health)
            {
                if (health != null)
                {
                    health.Died -= OnDied;
                    health.Revived -= OnRevived;
                }
                health = life;
                health.Died += OnDied;
                health.Revived += OnRevived;
            }
        }

        public void HideHeld()
        {
            if (fists != null) fists.Hide();
            if (held != null) held.Hide();
        }

        public void RestoreHeld()
        {
            if (shown == Shown.Weapon) PresentSword(weaponArt, weaponTint, weaponScale);
            else PresentFists();
        }

        void OnDied(Health _) => HideHeld();

        void OnRevived(Health _) => RestoreHeld();

        public void Equip(WeaponDefinition weapon)
        {
            Equip(weapon, null, DefaultTint);
        }

        public void Equip(WeaponDefinition weapon, Sprite art, Color tint)
        {
            if (weapon == null) return;
            if (inventory != null) inventory.Add(weapon);
            Equipped = weapon;
            if (stats != null)
                stats.SetWeaponMods(weapon.StrengthMod, weapon.DefenseMod, weapon.MagicMod, weapon.AgilityMod);
            if (combat != null)
                combat.SetLoadout(weapon.LightChain, weapon.HeavyChain, weapon.Aerial);
            weaponScale = 1f;
            if (fists != null) fists.Hide();
            ShowHeld(art, tint.a > 0.01f ? tint : DefaultTint);
        }

        public void PresentFists()
        {
            EnsureHeld();
            EnsureFists();
            shown = Shown.Fists;
            if (held != null) held.Hide();
            if (fists != null) fists.Show();
        }

        public void PresentSword(Sprite art, Color tint, float holdScale = 1f)
        {
            EnsureFists();
            if (fists != null) fists.Hide();
            EnsureHeld();
            weaponScale = holdScale;
            if (held != null) held.SetHoldScale(holdScale);
            ShowHeld(art, tint.a > 0.01f ? tint : DefaultTint);
        }

        public void BindCaster(AbilityCaster magic)
        {
            EnsureHeld();
            if (held != null) held.BindCaster(magic);
        }

        void EnsureHeld()
        {
            if (held != null) return;
            var weaponObject = new GameObject("Held Sword");
            weaponObject.transform.SetParent(HoldPoint(), false);
            held = weaponObject.AddComponent<HeldWeaponView>();
            held.Bind(GetComponent<Entity>(), combat);
        }

        void EnsureFists()
        {
            if (fists != null) return;
            var fistsObject = new GameObject("Fists");
            fistsObject.transform.SetParent(HoldPoint(), false);
            fists = fistsObject.AddComponent<HeldFistsView>();
            fists.Bind(GetComponent<Entity>(), combat);
        }

        Transform HoldPoint()
        {
            Transform body = transform.Find("Body");
            return body != null ? body : transform;
        }

        void ShowHeld(Sprite art, Color tint)
        {
            EnsureHeld();
            if (held == null) return;
            if (art == null)
            {
                art = PlaceholderSprites.Sword(
                    new Color(0.85f, 0.88f, 0.95f),
                    new Color(0.55f, 0.32f, 0.12f));
            }
            weaponArt = art;
            weaponTint = tint;
            shown = Shown.Weapon;
            held.Show(art, tint);
        }
    }
}
