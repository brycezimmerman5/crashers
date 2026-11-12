using System.Collections.Generic;
using Crashers.Characters;
using Crashers.Combat;
using UnityEngine;

namespace Crashers.Magic
{
    public sealed class AreaEffect : MonoBehaviour
    {
        readonly List<Hurtbox> overlap = new List<Hurtbox>();
        readonly HashSet<int> already = new HashSet<int>();

        Entity owner;
        AbilityDefinition ability;
        float groundX;
        float groundZ;
        float life;
        float duration = 0.32f;
        float outgoing;

        public void Launch(Entity source, float x, float z, AbilityDefinition definition, int magic)
        {
            owner = source;
            ability = definition;
            groundX = x;
            groundZ = z;
            duration = 0.34f;
            life = duration;
            outgoing = StatRules.OutgoingAbility(magic, definition != null ? definition.Damage : 10f);
            float radius = definition != null ? definition.Radius : 1.5f;
            transform.localScale = new Vector3(radius * 2f, radius * 1.15f, 1f);
            Motor2D5.Place(transform, groundX, groundZ, 0.05f);
        }

        void Update()
        {
            float dt = Time.deltaTime;
            life -= dt;
            var renderer = GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                Color color = renderer.color;
                color.a = Mathf.Clamp01(life / duration) * 0.75f;
                renderer.color = color;
            }
            if (ability == null || life <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            float radius = Mathf.Max(0.4f, ability.Radius);
            var volume = new HitVolume
            {
                CenterX = groundX,
                CenterZ = groundZ,
                HalfWidth = radius,
                HalfDepth = radius,
                HeightMin = 0f,
                HeightMax = 2.6f,
                Owner = owner
            };
            HitQuery.Collect(volume, overlap);
            for (int i = 0; i < overlap.Count; i++)
            {
                Hurtbox box = overlap[i];
                int id = box.GetInstanceID();
                if (already.Contains(id)) continue;
                already.Add(id);
                float sign = Mathf.Sign(box.WorldX - groundX);
                if (Mathf.Abs(sign) < 0.01f) sign = 1f;
                float zSign = Mathf.Sign(box.WorldZ - groundZ);
                if (Mathf.Abs(zSign) < 0.01f) zSign = 1f;
                HitResolver.Confirm(box, new HitInfo
                {
                    Attacker = owner,
                    OutgoingDamage = outgoing,
                    KnockbackX = sign * ability.Knockback,
                    KnockbackZ = zSign * ability.Knockback * 0.45f,
                    Hitstun = ability.Hitstun,
                    LaunchY = ability.Launch
                });
            }
        }
    }
}
