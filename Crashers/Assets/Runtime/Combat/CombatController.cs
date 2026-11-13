using System.Collections.Generic;
using Crashers.Characters;
using UnityEngine;

namespace Crashers.Combat
{
    public enum SwingPhase
    {
        Idle,
        Startup,
        Active,
        Recovery,
        ComboWindow
    }

    public sealed class CombatController : MonoBehaviour
    {
        Entity entity;
        Motor2D5 motor;
        CharacterStats stats;
        SpriteFlash flash;
        AttackDefinition[] lightChain = System.Array.Empty<AttackDefinition>();
        AttackDefinition[] heavyChain = System.Array.Empty<AttackDefinition>();
        AttackDefinition aerial;
        AttackDefinition current;
        SwingPhase phase = SwingPhase.Idle;
        float phaseTime;
        float phaseDuration;
        float cooldown;
        bool hasBuffer;
        AttackKind bufferedKind;
        float bufferUntil;
        readonly List<Hurtbox> overlap = new List<Hurtbox>();
        readonly HashSet<int> alreadyHit = new HashSet<int>();

        public event System.Action SwingStarted;
        public event System.Action SwingCancelled;

        public SwingPhase Phase => phase;
        public AttackKind ActiveKind => current != null ? current.Kind : AttackKind.Light;
        public float PhaseProgress
        {
            get
            {
                if (phaseDuration <= 0.0001f) return 0f;
                return Mathf.Clamp01(1f - phaseTime / phaseDuration);
            }
        }
        public bool IsBusy => phase == SwingPhase.Startup || phase == SwingPhase.Active || phase == SwingPhase.Recovery;

        public void Configure(Entity body, Motor2D5 mover, CharacterStats bodyStats, SpriteFlash sprite)
        {
            entity = body;
            motor = mover;
            stats = bodyStats;
            flash = sprite;
        }

        public void SetLoadout(AttackDefinition[] light, AttackDefinition[] heavy, AttackDefinition air)
        {
            lightChain = light ?? System.Array.Empty<AttackDefinition>();
            heavyChain = heavy ?? System.Array.Empty<AttackDefinition>();
            aerial = air;
        }

        public void BufferLight() => Buffer(AttackKind.Light);

        public void BufferHeavy() => Buffer(AttackKind.Heavy);

        public bool TryStartLight() => TryStart(AttackKind.Light);

        public bool TryStartHeavy() => TryStart(AttackKind.Heavy);

        public void Cancel()
        {
            bool swinging = phase == SwingPhase.Startup || phase == SwingPhase.Active || phase == SwingPhase.Recovery;
            phase = SwingPhase.Idle;
            phaseTime = 0f;
            phaseDuration = 0f;
            hasBuffer = false;
            alreadyHit.Clear();
            if (flash != null) flash.Release();
            if (swinging) SwingCancelled?.Invoke();
        }

        void Buffer(AttackKind kind)
        {
            hasBuffer = true;
            bufferedKind = kind;
            bufferUntil = Time.unscaledTime + 0.18f;
            if (CanAccept(kind))
            {
                hasBuffer = false;
                Begin(Resolve(kind));
            }
        }

        bool TryStart(AttackKind kind)
        {
            if (!CanAccept(kind)) return false;
            Begin(Resolve(kind));
            return true;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            if (cooldown > 0f) cooldown -= dt;
            TickPhase(dt);
            if (hasBuffer && Time.unscaledTime > bufferUntil) hasBuffer = false;
            if (hasBuffer && CanAccept(bufferedKind))
            {
                hasBuffer = false;
                Begin(Resolve(bufferedKind));
            }
        }

        void TickPhase(float dt)
        {
            if (phase == SwingPhase.Idle) return;
            if (phase == SwingPhase.Active) QueryHits();
            phaseTime -= dt;
            if (phaseTime > 0f) return;
            if (phase == SwingPhase.Startup) EnterActive();
            else if (phase == SwingPhase.Active) EnterRecovery();
            else if (phase == SwingPhase.Recovery) EnterComboOrIdle();
            else if (phase == SwingPhase.ComboWindow) FinishChain();
        }

        void Begin(AttackDefinition attack)
        {
            if (attack == null) return;
            current = attack;
            phase = SwingPhase.Startup;
            phaseTime = Mathf.Max(0.02f, attack.Startup);
            phaseDuration = phaseTime;
            alreadyHit.Clear();
            if (flash != null) flash.Hold(new Color(1f, 0.92f, 0.65f));
            SwingStarted?.Invoke();
        }

        void EnterActive()
        {
            phase = SwingPhase.Active;
            phaseTime = current != null ? Mathf.Max(0.04f, current.Active) : 0.04f;
            phaseDuration = phaseTime;
            alreadyHit.Clear();
            if (flash != null) flash.Hold(new Color(1f, 0.72f, 0.4f));
            QueryHits();
        }

        void EnterRecovery()
        {
            phase = SwingPhase.Recovery;
            phaseTime = current != null ? Mathf.Max(0.04f, current.Recovery) : 0.04f;
            phaseDuration = phaseTime;
            if (flash != null) flash.Release();
        }

        void EnterComboOrIdle()
        {
            if (current != null && current.Next != null && current.ComboWindow > 0.01f)
            {
                phase = SwingPhase.ComboWindow;
                phaseTime = current.ComboWindow;
                phaseDuration = phaseTime;
                return;
            }
            FinishChain();
        }

        void FinishChain()
        {
            phase = SwingPhase.Idle;
            phaseTime = 0f;
            phaseDuration = 0f;
            if (current != null) cooldown = Mathf.Max(0.05f, current.Cooldown);
            if (flash != null) flash.Release();
        }

        bool CanAccept(AttackKind kind)
        {
            if (entity != null && !entity.IsAlive) return false;
            if (motor != null && !motor.IsGrounded)
                return aerial != null && !IsBusy;
            if (IsBusy) return false;
            if (phase == SwingPhase.Idle && cooldown > 0f) return false;
            return Resolve(kind) != null;
        }

        AttackDefinition Resolve(AttackKind kind)
        {
            if (motor != null && !motor.IsGrounded && aerial != null) return aerial;
            if (phase == SwingPhase.ComboWindow && current != null && current.Kind == kind && current.Next != null)
                return current.Next;
            AttackDefinition[] chain = kind == AttackKind.Heavy ? heavyChain : lightChain;
            if (chain.Length == 0) return null;
            return chain[0];
        }

        void QueryHits()
        {
            if (current == null || motor == null || entity == null) return;
            HitVolume volume = HitQuery.FromAttack(current, entity, motor);
            HitQuery.Collect(volume, overlap);
            int strength = stats != null ? stats.Strength : 1;
            float outgoing = StatRules.OutgoingAttack(strength, current.DamageMultiplier);
            for (int i = 0; i < overlap.Count; i++)
            {
                Hurtbox box = overlap[i];
                int id = box.GetInstanceID();
                if (alreadyHit.Contains(id)) continue;
                alreadyHit.Add(id);
                float sign = Mathf.Sign(box.WorldX - motor.GroundX);
                if (Mathf.Abs(sign) < 0.01f) sign = entity.Facing;
                float zSign = Mathf.Sign(box.WorldZ - motor.GroundZ);
                if (Mathf.Abs(zSign) < 0.01f) zSign = 1f;
                HitResolver.Confirm(box, new HitInfo
                {
                    Attacker = entity,
                    OutgoingDamage = outgoing,
                    KnockbackX = sign * current.Knockback,
                    KnockbackZ = zSign * Mathf.Abs(current.KnockbackDepth),
                    Hitstun = current.Hitstun,
                    LaunchY = current.Launch
                });
            }
        }
    }
}
