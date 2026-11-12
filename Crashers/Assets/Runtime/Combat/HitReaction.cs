using Crashers.Characters;
using UnityEngine;

namespace Crashers.Combat
{
    public sealed class HitReaction : MonoBehaviour
    {
        const float KnockbackScale = 1.65f;
        const float KnockdownDuration = 0.65f;
        static readonly Color KnockdownTint = new Color(0.62f, 0.66f, 0.74f, 1f);

        Health health;
        Motor2D5 motor;
        SpriteFlash flash;
        Hurtbox hurtbox;
        float stun;
        float knockdownLeft;
        int hitsLanded;
        int knockdownEvery;
        bool launched;
        bool knockedDown;

        public bool IsLocked =>
            health != null && !health.IsAlive ||
            knockedDown ||
            stun > 0f ||
            (launched && motor != null && !motor.IsGrounded);

        public bool IsKnockedDown => knockedDown;
        public float KnockdownBlend { get; private set; }

        public event System.Action Stunned;

        public void Configure(Health body, Motor2D5 mover, SpriteFlash sprite, Hurtbox box)
        {
            health = body;
            motor = mover;
            flash = sprite;
            hurtbox = box;
        }

        public void SetKnockdownHits(int hits) => knockdownEvery = Mathf.Max(0, hits);

        public bool Receive(HitInfo info)
        {
            if (health == null || !health.IsAlive || knockedDown) return false;
            health.ApplyDamage(info.OutgoingDamage);
            if (!health.IsAlive) return true;
            stun = Mathf.Max(stun, info.Hitstun);
            if (info.LaunchY > 0.01f) launched = true;
            if (motor != null)
            {
                motor.ApplyImpulse(
                    info.KnockbackX * KnockbackScale,
                    info.KnockbackZ * KnockbackScale,
                    info.LaunchY);
                motor.SetControlLocked(true);
            }
            if (flash != null) flash.Flash();
            Stunned?.Invoke();
            if (knockdownEvery > 0)
            {
                hitsLanded++;
                if (hitsLanded >= knockdownEvery) BeginKnockdown(info);
            }
            return true;
        }

        public void ResetReaction()
        {
            stun = 0f;
            launched = false;
            hitsLanded = 0;
            EndKnockdown();
            if (motor != null && health != null && health.IsAlive) motor.SetControlLocked(false);
        }

        void BeginKnockdown(HitInfo info)
        {
            hitsLanded = 0;
            knockedDown = true;
            knockdownLeft = KnockdownDuration;
            KnockdownBlend = 1f;
            stun = Mathf.Max(stun, KnockdownDuration);
            if (hurtbox != null) hurtbox.SetAcceptsHits(false);
            if (flash != null) flash.Hold(KnockdownTint);
            if (motor == null) return;
            float sign = Mathf.Sign(info.KnockbackX);
            if (Mathf.Abs(sign) < 0.01f) sign = 1f;
            motor.ApplyImpulse(sign * 3.5f, 0f, 0f);
            motor.SetControlLocked(true);
        }

        void EndKnockdown()
        {
            if (!knockedDown && KnockdownBlend <= 0f) return;
            knockedDown = false;
            knockdownLeft = 0f;
            KnockdownBlend = 0f;
            stun = 0f;
            if (flash != null) flash.Release();
            if (hurtbox != null && health != null && health.IsAlive) hurtbox.SetAcceptsHits(true);
        }

        void Update()
        {
            if (health != null && !health.IsAlive)
            {
                if (motor != null) motor.SetControlLocked(true);
                return;
            }

            if (knockedDown)
            {
                knockdownLeft -= Time.deltaTime;
                const float rise = 0.16f;
                KnockdownBlend = knockdownLeft <= rise ? Mathf.Clamp01(knockdownLeft / rise) : 1f;
                if (knockdownLeft <= 0f) EndKnockdown();
            }
            else if (stun > 0f)
            {
                stun -= Time.deltaTime;
            }

            bool hold = knockedDown || stun > 0f || (launched && motor != null && !motor.IsGrounded);
            if (motor != null) motor.SetControlLocked(hold);
            if (hold) return;
            launched = false;
        }
    }
}
