using Crashers.Characters;
using Crashers.Combat;
using Crashers.Magic;
using UnityEngine;

namespace Crashers.Weapons
{
    /// <summary>
    /// Sword sprite parented to the fighter. Rest pose sits in the facing hand.
    /// Active frames sweep a short arc, then recovery returns to rest.
    /// </summary>
    public sealed class HeldWeaponView : MonoBehaviour
    {
        const float RestAngle = -18f;
        const float LightWindup = 32f;
        const float LightFollow = -62f;
        const float HeavyWindup = 50f;
        const float HeavyFollow = -108f;
        public const float HandForward = 0.34f;
        public const float HandHeight = 0.66f;

        SpriteRenderer sprite;
        DepthSorter sorter;
        Entity entity;
        CombatController combat;
        AbilityCaster caster;
        float holdScale = 1f;
        bool visible;

        void Awake()
        {
            sprite = GetComponent<SpriteRenderer>();
            if (sprite == null) sprite = gameObject.AddComponent<SpriteRenderer>();
            sorter = GetComponent<DepthSorter>();
            if (sorter == null) sorter = gameObject.AddComponent<DepthSorter>();
            sorter.Bias = 1;
            sprite.enabled = false;
            visible = false;
        }

        public void Bind(Entity body, CombatController attacks)
        {
            entity = body;
            combat = attacks;
        }

        public void BindCaster(AbilityCaster magic) => caster = magic;

        public void SetHoldScale(float scale) => holdScale = Mathf.Max(0.4f, scale);

        public void Show(Sprite art, Color tint)
        {
            if (sprite == null) return;
            sprite.sprite = art;
            sprite.color = tint;
            sprite.enabled = art != null;
            visible = art != null;
            ApplyPose();
        }

        public void Hide()
        {
            visible = false;
            if (sprite != null) sprite.enabled = false;
            transform.localRotation = Quaternion.identity;
        }

        void LateUpdate()
        {
            if (!visible) return;
            ApplyPose();
        }

        void ApplyPose()
        {
            if (entity == null) return;
            int facing = entity.Facing < 0 ? -1 : 1;
            float angle = PoseAngle();
            transform.localPosition = new Vector3(HandForward * facing, HandHeight, 0f);
            transform.localRotation = Quaternion.Euler(0f, 0f, angle * facing);
            transform.localScale = new Vector3(1.4f * holdScale, 1.2f * holdScale, 1f);
        }

        float PoseAngle()
        {
            if (combat != null && combat.IsBusy) return SwingAngle(combat);
            if (caster != null && caster.IsWindingUp) return CastAngle(caster.Windup01);
            return SwingAngle(combat);
        }

        static float CastAngle(float progress)
        {
            const float raised = 64f;
            const float released = -74f;
            if (progress < 0.62f)
            {
                float along = Mathf.SmoothStep(0f, 1f, progress / 0.62f);
                return Mathf.Lerp(RestAngle, raised, along);
            }
            float strike = Mathf.SmoothStep(0f, 1f, (progress - 0.62f) / 0.38f);
            return Mathf.Lerp(raised, released, strike);
        }

        public static float SwingAngle(CombatController combat)
        {
            if (combat == null) return RestAngle;
            bool heavy = combat.ActiveKind == AttackKind.Heavy;
            float windup = heavy ? HeavyWindup : LightWindup;
            float follow = heavy ? HeavyFollow : LightFollow;
            float along = Mathf.SmoothStep(0f, 1f, combat.PhaseProgress);
            if (combat.Phase == SwingPhase.Startup) return Mathf.Lerp(RestAngle, windup, along);
            if (combat.Phase == SwingPhase.Active) return Mathf.Lerp(windup, follow, along);
            if (combat.Phase == SwingPhase.Recovery) return Mathf.Lerp(follow, RestAngle, along);
            return RestAngle;
        }
    }
}
