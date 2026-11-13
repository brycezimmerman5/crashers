using Crashers.Combat;
using UnityEngine;

namespace Crashers.Characters
{
    /// <summary>
    /// Fires attack1 for a light swing and attack2 for a heavy swing.
    /// Idle is the controller default. The player root mirrors when he turns.
    /// </summary>
    public sealed class VikingPlayer : MonoBehaviour
    {
        const string LightTrigger = "attack1";
        const string HeavyTrigger = "attack2";

        Animator animator;
        CombatController combat;

        public void Bind(Entity body, Animator anim, CombatController attacks)
        {
            animator = anim;
            if (animator == null && body != null) animator = body.GetComponentInChildren<Animator>(true);
            combat = attacks;
            if (combat != null)
            {
                combat.SwingStarted += OnSwing;
                combat.SwingCancelled += OnCancel;
            }
        }

        void OnDestroy()
        {
            if (combat == null) return;
            combat.SwingStarted -= OnSwing;
            combat.SwingCancelled -= OnCancel;
        }

        void OnSwing()
        {
            if (animator == null) return;
            bool heavy = combat != null && combat.ActiveKind == AttackKind.Heavy;
            animator.ResetTrigger(LightTrigger);
            animator.ResetTrigger(HeavyTrigger);
            animator.SetTrigger(heavy ? HeavyTrigger : LightTrigger);
        }

        void OnCancel()
        {
            if (animator == null) return;
            animator.ResetTrigger(LightTrigger);
            animator.ResetTrigger(HeavyTrigger);
            int idle = Animator.StringToHash("idle");
            if (animator.HasState(0, idle)) animator.Play(idle, 0, 0f);
        }
    }
}
