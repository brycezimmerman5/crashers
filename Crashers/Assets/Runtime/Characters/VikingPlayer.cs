using Crashers.Combat;
using Crashers.Content;
using UnityEngine;

namespace Crashers.Characters
{
    /// <summary>
    /// Faces the viking rig and fires the attack1 trigger when a swing starts.
    /// Idle is the controller default, so it plays with no extra trigger.
    /// </summary>
    public sealed class VikingPlayer : MonoBehaviour
    {
        Entity entity;
        Animator animator;
        CombatController combat;

        public void Bind(Entity body, Animator anim, CombatController attacks)
        {
            entity = body;
            animator = anim;
            combat = attacks;
            if (combat != null)
            {
                combat.SwingStarted += OnSwing;
                combat.SwingCancelled += OnCancel;
            }
            ApplyFacing();
        }

        void OnDestroy()
        {
            if (combat == null) return;
            combat.SwingStarted -= OnSwing;
            combat.SwingCancelled -= OnCancel;
        }

        void OnSwing()
        {
            if (animator != null) animator.SetTrigger(VikingArt.AttackTrigger);
        }

        void OnCancel()
        {
            if (animator == null) return;
            animator.ResetTrigger(VikingArt.AttackTrigger);
            animator.Play(VikingArt.IdleState, 0, 0f);
        }

        void LateUpdate()
        {
            ApplyFacing();
        }

        void ApplyFacing()
        {
            float sign = entity != null && entity.Facing < 0 ? -1f : 1f;
            transform.localScale = new Vector3(sign, 1f, 1f);
        }
    }
}
