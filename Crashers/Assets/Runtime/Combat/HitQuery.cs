using System.Collections.Generic;
using Crashers.Characters;
using UnityEngine;

namespace Crashers.Combat
{
    public struct HitVolume
    {
        public float CenterX;
        public float CenterZ;
        public float HalfWidth;
        public float HalfDepth;
        public float HeightMin;
        public float HeightMax;
        public Entity Owner;
    }

    public struct HitInfo
    {
        public Entity Attacker;
        public float OutgoingDamage;
        public float KnockbackX;
        public float KnockbackZ;
        public float Hitstun;
        public float LaunchY;
    }

    public static class HitQuery
    {
        public static bool Overlaps(HitVolume volume, Hurtbox box)
        {
            if (box == null) return false;
            if (Mathf.Abs(volume.CenterX - box.WorldX) > volume.HalfWidth + box.HalfWidth) return false;
            if (Mathf.Abs(volume.CenterZ - box.WorldZ) > volume.HalfDepth + box.HalfDepth) return false;
            return volume.HeightMin <= box.WorldHeightMax && box.WorldHeightMin <= volume.HeightMax;
        }

        public static void Collect(HitVolume volume, List<Hurtbox> into)
        {
            into.Clear();
            var all = Hurtbox.Active;
            for (int i = 0; i < all.Count; i++)
            {
                Hurtbox box = all[i];
                if (box == null || !box.AcceptsHits) continue;
                Entity other = box.Owner;
                if (other == null || !other.IsAlive) continue;
                if (volume.Owner != null && (other == volume.Owner || other.Faction == volume.Owner.Faction)) continue;
                if (!Overlaps(volume, box)) continue;
                into.Add(box);
            }
        }

        public static HitVolume FromAttack(AttackDefinition attack, Entity attacker, Motor2D5 motor)
        {
            float facing = attacker != null && attacker.Facing < 0 ? -1f : 1f;
            float x = motor.GroundX + attack.HitboxOffset.x * facing;
            float z = motor.GroundZ + attack.HitboxOffset.z;
            float mid = motor.JumpHeight + attack.HitboxOffset.y;
            float halfH = attack.HitboxSize.y * 0.5f;
            return new HitVolume
            {
                CenterX = x,
                CenterZ = z,
                HalfWidth = attack.HitboxSize.x * 0.5f,
                HalfDepth = attack.HitboxDepth * 0.5f,
                HeightMin = mid - halfH,
                HeightMax = mid + halfH,
                Owner = attacker
            };
        }
    }

    public static class HitResolver
    {
        public static void Confirm(Hurtbox box, HitInfo info)
        {
            if (box == null) return;
            HitReaction reaction = box.GetComponent<HitReaction>();
            if (reaction == null || !reaction.Receive(info)) return;
            BloodBurst.Spawn(box, info.Attacker);
            if (HitStop.Shared != null) HitStop.Shared.Punch();
            if (CameraShake.Shared != null) CameraShake.Shared.Shake(0.12f, 0.12f);
        }
    }
}
