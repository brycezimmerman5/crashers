using UnityEngine;

namespace Crashers.Enemies
{
    /// <summary>
    /// Hands melee and boss enemies distinct offsets around the player so they do not stack.
    /// Ranged enemies hold their preferred distance instead of taking a melee slot.
    /// </summary>
    public sealed class EnemySlotAssigner : MonoBehaviour
    {
        static readonly Vector2[] Slots =
        {
            new Vector2(-1.15f, 0.42f),
            new Vector2(1.15f, -0.36f),
            new Vector2(-1.3f, -0.48f),
            new Vector2(1.3f, 0.5f)
        };

        int nextSlot;

        public int TakeSlot()
        {
            int slot = nextSlot;
            nextSlot++;
            return slot;
        }

        public Vector2 Desired(EnemyBrain brain)
        {
            if (brain == null || brain.TargetMotor == null || brain.Motor == null) return Vector2.zero;
            float px = brain.TargetMotor.GroundX;
            float pz = brain.TargetMotor.GroundZ;
            float strafe = Mathf.Sin(Time.time * 1.6f + brain.StrafePhase) * 0.42f;
            if (brain.Archetype != null && brain.Archetype.Role == EnemyRole.Ranged)
            {
                float dir = Mathf.Sign(brain.Motor.GroundX - px);
                if (Mathf.Abs(dir) < 0.01f) dir = 1f;
                float range = brain.Archetype.PreferredRange;
                return new Vector2(px + dir * range, pz + strafe);
            }

            int index = brain.SlotIndex;
            if (index < 0) index = 0;
            Vector2 slot = Slots[index % Slots.Length];
            return new Vector2(px + slot.x, pz + slot.y + strafe * 0.35f);
        }
    }
}
