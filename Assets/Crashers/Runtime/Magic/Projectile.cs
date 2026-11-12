using System.Collections.Generic;
using Crashers.Characters;
using Crashers.Combat;
using UnityEngine;

namespace Crashers.Magic
{
    public sealed class Projectile : MonoBehaviour
    {
        readonly List<Hurtbox> overlap = new List<Hurtbox>();

        Entity owner;
        AbilityDefinition ability;
        Vector2 direction;
        float groundX;
        float groundZ;
        float height;
        float speed;
        float life;
        float outgoing;

        public void Launch(Entity source, float x, float z, float shotHeight, Vector2 aim, AbilityDefinition definition, int magic)
        {
            owner = source;
            ability = definition;
            groundX = x;
            groundZ = z;
            height = shotHeight;
            direction = aim.sqrMagnitude < 0.001f ? Vector2.right : aim.normalized;
            speed = definition != null ? definition.Speed : 8f;
            life = 1.35f;
            outgoing = StatRules.OutgoingAbility(magic, definition != null ? definition.Damage : 8f);
            Motor2D5.Place(transform, groundX, groundZ, height);
        }

        void Update()
        {
            float dt = Time.deltaTime;
            life -= dt;
            groundX += direction.x * speed * dt;
            groundZ += direction.y * speed * dt;
            Motor2D5.Place(transform, groundX, groundZ, height);
            if (ability == null || life <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            float radius = Mathf.Max(0.2f, ability.Radius);
            var volume = new HitVolume
            {
                CenterX = groundX,
                CenterZ = groundZ,
                HalfWidth = radius,
                HalfDepth = radius + 0.15f,
                HeightMin = height - 0.55f,
                HeightMax = height + 0.55f,
                Owner = owner
            };
            HitQuery.Collect(volume, overlap);
            if (overlap.Count == 0) return;
            for (int i = 0; i < overlap.Count; i++)
            {
                Hurtbox box = overlap[i];
                float sign = Mathf.Sign(box.WorldX - groundX);
                if (Mathf.Abs(sign) < 0.01f) sign = direction.x >= 0f ? 1f : -1f;
                float zSign = Mathf.Sign(box.WorldZ - groundZ);
                if (Mathf.Abs(zSign) < 0.01f) zSign = 1f;
                HitResolver.Confirm(box, new HitInfo
                {
                    Attacker = owner,
                    OutgoingDamage = outgoing,
                    KnockbackX = sign * ability.Knockback,
                    KnockbackZ = zSign * ability.Knockback * 0.35f,
                    Hitstun = ability.Hitstun,
                    LaunchY = ability.Launch
                });
            }
            Destroy(gameObject);
        }
    }
}
