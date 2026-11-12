using Crashers.Characters;
using Crashers.Combat;
using Crashers.Content;
using UnityEngine;

namespace Crashers.Weapons
{
    /// <summary>
    /// Two fist sprites on the same hand pivot and swing arc as the sword.
    /// </summary>
    public sealed class HeldFistsView : MonoBehaviour
    {
        Entity entity;
        CombatController combat;
        SpriteRenderer lead;
        SpriteRenderer rear;
        bool visible;

        void Awake()
        {
            Sprite art = PlaceholderSprites.Fist();
            lead = CreateFist(transform, "Lead Fist", art, new Vector3(0.04f, 0.3f, 0f), 2);
            rear = CreateFist(transform, "Rear Fist", art, new Vector3(-0.14f, 0.12f, 0f), 1);
            Hide();
        }

        public void Bind(Entity body, CombatController attacks)
        {
            entity = body;
            combat = attacks;
        }

        public void Show()
        {
            visible = true;
            SetRenderers(true);
            ApplyPose();
        }

        public void Hide()
        {
            visible = false;
            SetRenderers(false);
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
            float angle = HeldWeaponView.SwingAngle(combat);
            transform.localPosition = new Vector3(HeldWeaponView.HandForward * facing, HeldWeaponView.HandHeight, 0f);
            transform.localRotation = Quaternion.Euler(0f, 0f, angle * facing);
        }

        void SetRenderers(bool enabled)
        {
            if (lead != null) lead.enabled = enabled;
            if (rear != null) rear.enabled = enabled;
        }

        static SpriteRenderer CreateFist(Transform parent, string fistName, Sprite art, Vector3 localPosition, int sortBias)
        {
            var fistObject = new GameObject(fistName);
            fistObject.transform.SetParent(parent, false);
            fistObject.transform.localPosition = localPosition;
            var renderer = fistObject.AddComponent<SpriteRenderer>();
            renderer.sprite = art;
            renderer.enabled = false;
            var sorter = fistObject.AddComponent<DepthSorter>();
            sorter.Bias = sortBias;
            return renderer;
        }
    }
}
