using UnityEngine;

namespace Crashers.Combat
{
    public enum AttackKind
    {
        Light = 0,
        Heavy = 1
    }

    [CreateAssetMenu(menuName = "Crashers/Attack Definition", fileName = "Attack")]
    public sealed class AttackDefinition : ScriptableObject
    {
        public string Id = "attack";
        public AttackKind Kind;
        public int ChainIndex;
        public float Startup = 0.08f;
        public float Active = 0.1f;
        public float Recovery = 0.14f;
        public float DamageMultiplier = 1f;
        public float Knockback = 3f;
        public float KnockbackDepth = 0.35f;
        public float Hitstun = 0.18f;
        public float Launch;
        public bool Aerial;
        public Vector3 HitboxOffset = new Vector3(0.8f, 0.7f, 0f);
        public Vector2 HitboxSize = new Vector2(1.05f, 0.85f);
        public float HitboxDepth = 0.95f;
        public float Cooldown = 0.28f;
        public float ComboWindow = 0.34f;
        public AttackDefinition Next;
    }
}
