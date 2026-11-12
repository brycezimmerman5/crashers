using UnityEngine;

namespace Crashers.Enemies
{
    public enum EnemyStateId
    {
        Idle,
        Chase,
        Attack,
        Hurt,
        Retreat,
        Death
    }

    public interface IEnemyState
    {
        EnemyStateId Id { get; }
        void Enter(EnemyBrain brain);
        void Tick(EnemyBrain brain, float dt);
        void Exit(EnemyBrain brain);
    }

    public sealed class IdleState : IEnemyState
    {
        float timer;

        public EnemyStateId Id => EnemyStateId.Idle;

        public void Enter(EnemyBrain brain)
        {
            timer = 0.45f;
            brain.Motor.SetMoveInput(Vector2.zero);
        }

        public void Tick(EnemyBrain brain, float dt)
        {
            brain.Motor.SetMoveInput(Vector2.zero);
            timer -= dt;
            if (!brain.TargetAlive) return;
            brain.FaceTarget();
            float aggro = brain.Archetype != null ? brain.Archetype.AggroRange : 8f;
            if (timer <= 0f && brain.TargetDistance() <= aggro)
                brain.ChangeState(EnemyStateId.Chase);
        }

        public void Exit(EnemyBrain brain)
        {
            timer = 0f;
        }
    }

    public sealed class ChaseState : IEnemyState
    {
        public EnemyStateId Id => EnemyStateId.Chase;

        public void Enter(EnemyBrain brain) => brain.FaceTarget();

        public void Tick(EnemyBrain brain, float dt)
        {
            if (!brain.TargetAlive)
            {
                brain.ChangeState(EnemyStateId.Idle);
                return;
            }
            if (brain.WantsRetreat)
            {
                brain.ChangeState(EnemyStateId.Retreat);
                return;
            }
            brain.FaceTarget();
            brain.MoveTowards(brain.DesiredPosition());
            if (brain.InAttackRange()) brain.ChangeState(EnemyStateId.Attack);
        }

        public void Exit(EnemyBrain brain) => brain.Motor.SetMoveInput(Vector2.zero);
    }

    public sealed class AttackState : IEnemyState
    {
        int step;
        float gap;

        public EnemyStateId Id => EnemyStateId.Attack;

        public void Enter(EnemyBrain brain)
        {
            step = 0;
            gap = 0.05f;
            brain.Motor.SetMoveInput(Vector2.zero);
            brain.FaceTarget();
        }

        public void Tick(EnemyBrain brain, float dt)
        {
            brain.Motor.SetMoveInput(Vector2.zero);
            if (!brain.TargetAlive)
            {
                brain.ChangeState(EnemyStateId.Idle);
                return;
            }
            brain.FaceTarget();
            if (brain.WantsRetreat && (brain.Combat == null || !brain.Combat.IsBusy))
            {
                brain.ChangeState(EnemyStateId.Retreat);
                return;
            }

            if (brain.Archetype != null && brain.Archetype.Role == EnemyRole.Ranged)
            {
                TickRanged(brain, dt);
                return;
            }

            if (brain.Combat != null && brain.Combat.IsBusy) return;
            if (!brain.InAttackRange())
            {
                brain.ChangeState(EnemyStateId.Chase);
                return;
            }
            if (gap > 0f)
            {
                gap -= dt;
                return;
            }

            PatternStep[] pattern = brain.Archetype != null ? brain.Archetype.Pattern : null;
            if (pattern == null || step >= pattern.Length)
            {
                brain.ChangeState(EnemyStateId.Chase);
                return;
            }

            PatternStep command = pattern[step];
            bool started = false;
            if (command == PatternStep.Heavy)
            {
                started = brain.Combat != null && brain.Combat.TryStartHeavy();
            }
            else if (command == PatternStep.Ability)
            {
                bool canCast = brain.Caster != null && brain.Archetype.RangedAbility != null;
                started = canCast && brain.Caster.Cast(brain.Archetype.RangedAbility);
                if (!started)
                {
                    brain.ChangeState(EnemyStateId.Chase);
                    return;
                }
            }
            else
            {
                started = brain.Combat != null && brain.Combat.TryStartLight();
            }

            if (!started) return;
            step++;
            gap = 0.06f;
        }

        void TickRanged(EnemyBrain brain, float dt)
        {
            if (brain.Caster != null && brain.Caster.IsCasting) return;
            if (!brain.InAttackRange())
            {
                brain.ChangeState(EnemyStateId.Chase);
                return;
            }
            if (gap > 0f)
            {
                gap -= dt;
                return;
            }
            var ability = brain.Archetype.RangedAbility;
            if (ability != null && brain.Caster != null && brain.Caster.Cast(ability))
            {
                brain.ChangeState(EnemyStateId.Chase);
                return;
            }
            float strafe = Mathf.Sin(Time.time * 1.8f + brain.StrafePhase);
            brain.Motor.SetMoveInput(new Vector2(0f, strafe));
        }

        public void Exit(EnemyBrain brain)
        {
            step = 0;
            gap = 0f;
        }
    }

    public sealed class HurtState : IEnemyState
    {
        public EnemyStateId Id => EnemyStateId.Hurt;

        public void Enter(EnemyBrain brain)
        {
            brain.Motor.SetMoveInput(Vector2.zero);
            if (brain.Combat != null) brain.Combat.Cancel();
        }

        public void Tick(EnemyBrain brain, float dt)
        {
            brain.Motor.SetMoveInput(Vector2.zero);
            if (brain.Health != null && !brain.Health.IsAlive)
            {
                brain.ChangeState(EnemyStateId.Death);
                return;
            }
            if (brain.Reaction != null && brain.Reaction.IsLocked) return;
            brain.ChangeState(brain.WantsRetreat ? EnemyStateId.Retreat : EnemyStateId.Chase);
        }

        public void Exit(EnemyBrain brain) => brain.Motor.SetMoveInput(Vector2.zero);
    }

    public sealed class RetreatState : IEnemyState
    {
        float timer;

        public EnemyStateId Id => EnemyStateId.Retreat;

        public void Enter(EnemyBrain brain)
        {
            timer = 1.15f;
            brain.FaceTarget();
        }

        public void Tick(EnemyBrain brain, float dt)
        {
            timer -= dt;
            if (!brain.TargetAlive)
            {
                brain.ChangeState(EnemyStateId.Idle);
                return;
            }
            brain.FaceTarget();
            float away = brain.Self != null && brain.Self.Facing > 0 ? -1f : 1f;
            float depth = Mathf.Sin(Time.time * 2.2f + brain.StrafePhase) * 0.65f;
            brain.Motor.SetMoveInput(new Vector2(away, depth));
            float farEnough = brain.Archetype != null ? brain.Archetype.PreferredRange * 2.1f : 4f;
            if (timer <= 0f || brain.TargetDistance() >= farEnough)
                brain.ChangeState(EnemyStateId.Chase);
        }

        public void Exit(EnemyBrain brain)
        {
            brain.ArmRetreatCooldown(4.5f);
            brain.Motor.SetMoveInput(Vector2.zero);
        }
    }

    public sealed class DeathState : IEnemyState
    {
        float timer;

        public EnemyStateId Id => EnemyStateId.Death;

        public void Enter(EnemyBrain brain)
        {
            timer = 0f;
            brain.Motor.SetMoveInput(Vector2.zero);
            brain.Motor.SetControlLocked(true);
            if (brain.Body != null) brain.Body.SetAcceptsHits(false);
            if (brain.Flash != null) brain.Flash.SetDead();
            brain.NotifyKilled();
            Pose(brain, 1f);
        }

        public void Tick(EnemyBrain brain, float dt)
        {
            brain.Motor.SetMoveInput(Vector2.zero);
            timer += dt;
            float blend = Mathf.Clamp01(timer / 0.2f);
            Pose(brain, blend);
        }

        public void Exit(EnemyBrain brain) => brain.Motor.SetMoveInput(Vector2.zero);

        static void Pose(EnemyBrain brain, float blend)
        {
            if (brain.Visual == null) return;
            float sign = brain.Self != null ? brain.Self.Facing : 1f;
            brain.Visual.localRotation = Quaternion.Euler(0f, 0f, -78f * sign * blend);
        }
    }
}
