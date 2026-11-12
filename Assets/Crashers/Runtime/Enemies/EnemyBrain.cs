using System;
using Crashers.Characters;
using Crashers.Combat;
using Crashers.Magic;
using UnityEngine;

namespace Crashers.Enemies
{
    public sealed class EnemyBrain : MonoBehaviour
    {
        readonly IdleState idle = new IdleState();
        readonly ChaseState chase = new ChaseState();
        readonly AttackState attack = new AttackState();
        readonly HurtState hurt = new HurtState();
        readonly RetreatState retreat = new RetreatState();
        readonly DeathState death = new DeathState();

        IEnemyState current;
        float retreatCooldown;
        bool rewarded;

        public EnemyArchetype Archetype { get; private set; }
        public Entity Self { get; private set; }
        public Entity Target { get; private set; }
        public Motor2D5 Motor { get; private set; }
        public Motor2D5 TargetMotor { get; private set; }
        public CombatController Combat { get; private set; }
        public AbilityCaster Caster { get; private set; }
        public Health Health { get; private set; }
        public HitReaction Reaction { get; private set; }
        public Hurtbox Body { get; private set; }
        public SpriteFlash Flash { get; private set; }
        public Transform Visual { get; private set; }
        public EnemySlotAssigner Slots { get; private set; }
        public int SlotIndex { get; private set; }
        public float StrafePhase { get; private set; }
        public EnemyStateId State => current != null ? current.Id : EnemyStateId.Idle;

        public event Action<EnemyBrain> Killed;

        public void Configure(
            EnemyArchetype archetype,
            Entity self,
            Entity target,
            Motor2D5 motor,
            Motor2D5 targetMotor,
            CombatController combat,
            AbilityCaster caster,
            Health health,
            HitReaction reaction,
            Hurtbox body,
            SpriteFlash flash,
            Transform visual,
            EnemySlotAssigner slots,
            int slotIndex)
        {
            Archetype = archetype;
            Self = self;
            Target = target;
            Motor = motor;
            TargetMotor = targetMotor;
            Combat = combat;
            Caster = caster;
            Health = health;
            Reaction = reaction;
            Body = body;
            Flash = flash;
            Visual = visual;
            Slots = slots;
            SlotIndex = slotIndex;
            StrafePhase = slotIndex * 1.7f + 0.4f;
            if (reaction != null) reaction.Stunned += OnStunned;
            if (health != null) health.Died += OnDied;
            ChangeState(EnemyStateId.Idle);
        }

        public bool TargetAlive => Target != null && Target.IsAlive && TargetMotor != null;

        public float HealthFraction => Health != null ? Health.Fraction : 1f;

        public bool WantsRetreat =>
            Archetype != null &&
            HealthFraction <= Archetype.RetreatHealthFraction &&
            retreatCooldown <= 0f &&
            Health != null &&
            Health.IsAlive;

        public float TargetDistance()
        {
            if (!TargetAlive) return 99f;
            float dx = Motor.GroundX - TargetMotor.GroundX;
            float dz = Motor.GroundZ - TargetMotor.GroundZ;
            return Mathf.Sqrt(dx * dx + dz * dz);
        }

        public bool InAttackRange()
        {
            if (!TargetAlive || Archetype == null) return false;
            float dx = Mathf.Abs(Motor.GroundX - TargetMotor.GroundX);
            float dz = Mathf.Abs(Motor.GroundZ - TargetMotor.GroundZ);
            if (Archetype.Role == EnemyRole.Ranged)
            {
                float band = Archetype.PreferredRange;
                return dx >= band * 0.62f && dx <= band * 1.28f && dz <= 0.85f;
            }
            return dx <= Archetype.PreferredRange + 0.2f && dz <= 0.75f;
        }

        public Vector2 DesiredPosition()
        {
            if (Slots == null || !TargetAlive) return new Vector2(Motor.GroundX, Motor.GroundZ);
            return Slots.Desired(this);
        }

        public void MoveTowards(Vector2 point)
        {
            float dx = point.x - Motor.GroundX;
            float dz = point.y - Motor.GroundZ;
            var delta = new Vector2(dx, dz);
            if (delta.sqrMagnitude < 0.012f) Motor.SetMoveInput(Vector2.zero);
            else Motor.SetMoveInput(delta.normalized);
        }

        public void FaceTarget()
        {
            if (!TargetAlive || Self == null) return;
            Self.FaceTowards(TargetMotor.GroundX, Motor.GroundX);
        }

        public void ArmRetreatCooldown(float seconds) => retreatCooldown = seconds;

        public void NotifyKilled()
        {
            if (rewarded) return;
            rewarded = true;
            Killed?.Invoke(this);
        }

        public void ChangeState(EnemyStateId id)
        {
            IEnemyState next = StateFor(id);
            if (next == null || next == current) return;
            if (current != null) current.Exit(this);
            current = next;
            current.Enter(this);
        }

        void Update()
        {
            if (retreatCooldown > 0f) retreatCooldown -= Time.deltaTime;
            if (current != null) current.Tick(this, Time.deltaTime);
        }

        void OnStunned()
        {
            if (Health != null && !Health.IsAlive) return;
            if (State == EnemyStateId.Death) return;
            ChangeState(EnemyStateId.Hurt);
        }

        void OnDied(Health _)
        {
            if (Combat != null) Combat.Cancel();
            if (Body != null) Body.SetAcceptsHits(false);
            ChangeState(EnemyStateId.Death);
        }

        IEnemyState StateFor(EnemyStateId id)
        {
            switch (id)
            {
                case EnemyStateId.Idle: return idle;
                case EnemyStateId.Chase: return chase;
                case EnemyStateId.Attack: return attack;
                case EnemyStateId.Hurt: return hurt;
                case EnemyStateId.Retreat: return retreat;
                case EnemyStateId.Death: return death;
                default: return idle;
            }
        }
    }
}
