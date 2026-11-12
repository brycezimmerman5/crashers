using Crashers.Characters;
using UnityEngine;

namespace Crashers.Combat
{
    public sealed class HitReaction : MonoBehaviour
    {
        Health health;
        Motor2D5 motor;
        SpriteFlash flash;
        float stun;
        bool launched;

        public bool IsLocked =>
            health != null && !health.IsAlive ||
            stun > 0f ||
            (launched && motor != null && !motor.IsGrounded);

        public event System.Action Stunned;

        public void Configure(Health body, Motor2D5 mover, SpriteFlash sprite)
        {
            health = body;
            motor = mover;
            flash = sprite;
        }

        public bool Receive(HitInfo info)
        {
            if (health == null || !health.IsAlive) return false;
            health.ApplyDamage(info.OutgoingDamage);
            if (!health.IsAlive) return true;
            stun = Mathf.Max(stun, info.Hitstun);
            if (info.LaunchY > 0.01f) launched = true;
            if (motor != null)
            {
                motor.ApplyImpulse(info.KnockbackX, info.KnockbackZ, info.LaunchY);
                motor.SetControlLocked(true);
            }
            if (flash != null) flash.Flash();
            Stunned?.Invoke();
            return true;
        }

        public void ResetReaction()
        {
            stun = 0f;
            launched = false;
            if (motor != null && health != null && health.IsAlive) motor.SetControlLocked(false);
        }

        void Update()
        {
            if (health != null && !health.IsAlive)
            {
                if (motor != null) motor.SetControlLocked(true);
                return;
            }
            if (stun > 0f) stun -= Time.deltaTime;
            bool hold = stun > 0f || (launched && motor != null && !motor.IsGrounded);
            if (motor != null) motor.SetControlLocked(hold);
            if (hold) return;
            launched = false;
        }
    }
}
