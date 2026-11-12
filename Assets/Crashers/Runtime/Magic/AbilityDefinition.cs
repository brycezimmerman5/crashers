using UnityEngine;

namespace Crashers.Magic
{
    public enum Element
    {
        None = 0,
        Fire = 1,
        Ice = 2,
        Lightning = 3,
        Nature = 4
    }

    public enum DeliveryKind
    {
        Projectile = 0,
        Area = 1
    }

    [CreateAssetMenu(menuName = "Crashers/Ability Definition", fileName = "Ability")]
    public sealed class AbilityDefinition : ScriptableObject
    {
        public string Id = "ability";
        public string DisplayName = "Ability";
        public Element Element = Element.None;
        public float ManaCost = 8f;
        public float Cooldown = 0.6f;
        public DeliveryKind Delivery = DeliveryKind.Projectile;
        public float Damage = 12f;
        public float Radius = 0.4f;
        public float Speed = 9f;
        public float Knockback = 3f;
        public float Hitstun = 0.2f;
        public float Launch;
    }
}
