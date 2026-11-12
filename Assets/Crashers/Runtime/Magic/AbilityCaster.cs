using System.Collections.Generic;
using Crashers.Characters;
using Crashers.Combat;
using Crashers.Progression;
using UnityEngine;

namespace Crashers.Magic
{
    public sealed class AbilityCaster : MonoBehaviour
    {
        readonly Dictionary<string, float> readyAt = new Dictionary<string, float>();

        Entity entity;
        Motor2D5 motor;
        CharacterStats stats;
        Mana mana;
        Unlocks unlocks;
        Sprite projectileSprite;
        Sprite areaSprite;
        AbilityDefinition slot0;
        AbilityDefinition slot1;
        AbilityDefinition slot2;
        float busy;

        public bool IsCasting => busy > 0f;

        public void Configure(
            Entity body,
            Motor2D5 mover,
            CharacterStats bodyStats,
            Mana pool,
            Unlocks known,
            Sprite projectile,
            Sprite area)
        {
            entity = body;
            motor = mover;
            stats = bodyStats;
            mana = pool;
            unlocks = known;
            projectileSprite = projectile;
            areaSprite = area;
        }

        public void SetSlots(AbilityDefinition first, AbilityDefinition second, AbilityDefinition third)
        {
            slot0 = first;
            slot1 = second;
            slot2 = third;
        }

        public bool TryCastSlot(int index)
        {
            AbilityDefinition ability = index == 0 ? slot0 : index == 1 ? slot1 : slot2;
            if (ability == null) return false;
            if (unlocks != null && !unlocks.IsUnlocked(ability.Id)) return false;
            return Cast(ability);
        }

        public bool Cast(AbilityDefinition ability)
        {
            if (ability == null || entity == null || !entity.IsAlive || motor == null) return false;
            if (busy > 0f) return false;
            float ready;
            if (readyAt.TryGetValue(ability.Id, out ready) && Time.unscaledTime < ready) return false;
            if (mana != null && !mana.TrySpend(ability.ManaCost)) return false;
            if (ability.Delivery == DeliveryKind.Area) SpawnArea(ability);
            else SpawnProjectile(ability);
            readyAt[ability.Id] = Time.unscaledTime + Mathf.Max(0.05f, ability.Cooldown);
            busy = 0.12f;
            return true;
        }

        void Update()
        {
            if (busy > 0f) busy -= Time.deltaTime;
        }

        void SpawnProjectile(AbilityDefinition ability)
        {
            var go = new GameObject(ability.DisplayName);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = projectileSprite;
            renderer.color = ElementColor(ability.Element);
            go.AddComponent<DepthSorter>();
            var shot = go.AddComponent<Projectile>();
            float facing = entity.Facing >= 0 ? 1f : -1f;
            Vector2 direction = Aim(facing);
            int magic = stats != null ? stats.Magic : 1;
            shot.Launch(
                entity,
                motor.GroundX + facing * 0.55f,
                motor.GroundZ,
                motor.JumpHeight + 0.75f,
                direction,
                ability,
                magic);
        }

        void SpawnArea(AbilityDefinition ability)
        {
            var go = new GameObject(ability.DisplayName);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = areaSprite;
            renderer.color = ElementColor(ability.Element);
            go.AddComponent<DepthSorter>();
            var area = go.AddComponent<AreaEffect>();
            int magic = stats != null ? stats.Magic : 1;
            area.Launch(entity, motor.GroundX, motor.GroundZ, ability, magic);
        }

        Vector2 Aim(float facing)
        {
            float best = 999f;
            float targetZ = motor.GroundZ;
            var boxes = Hurtbox.Active;
            for (int i = 0; i < boxes.Count; i++)
            {
                Hurtbox box = boxes[i];
                if (box == null || !box.AcceptsHits || box.Owner == null || !box.Owner.IsAlive) continue;
                if (box.Owner.Faction == entity.Faction) continue;
                float dx = box.WorldX - motor.GroundX;
                if (Mathf.Sign(dx) != facing && Mathf.Abs(dx) > 0.35f) continue;
                float score = Mathf.Abs(dx) + Mathf.Abs(box.WorldZ - motor.GroundZ);
                if (score >= best) continue;
                best = score;
                targetZ = box.WorldZ;
            }
            float dz = Mathf.Clamp(targetZ - motor.GroundZ, -1.25f, 1.25f);
            var direction = new Vector2(facing, dz);
            if (direction.sqrMagnitude < 0.0001f) direction = new Vector2(facing, 0f);
            return direction.normalized;
        }

        public static Color ElementColor(Element element)
        {
            switch (element)
            {
                case Element.Fire: return new Color(1f, 0.45f, 0.12f, 0.9f);
                case Element.Ice: return new Color(0.45f, 0.85f, 1f, 0.9f);
                case Element.Lightning: return new Color(1f, 0.92f, 0.25f, 0.95f);
                case Element.Nature: return new Color(0.35f, 0.9f, 0.4f, 0.85f);
                default: return new Color(0.9f, 0.9f, 0.95f, 0.9f);
            }
        }
    }
}
