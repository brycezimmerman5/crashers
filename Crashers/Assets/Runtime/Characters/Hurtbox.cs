using System.Collections.Generic;
using UnityEngine;

namespace Crashers.Characters
{
    public sealed class Hurtbox : MonoBehaviour
    {
        static readonly List<Hurtbox> active = new List<Hurtbox>();

        Motor2D5 motor;
        bool accepts = true;

        public static IReadOnlyList<Hurtbox> Active => active;

        float baseHalfWidth = 0.28f;
        float baseHalfDepth = 0.32f;
        float baseHeightMin = 0.05f;
        float baseHeightMax = 1.35f;
        float bodyScale = 1f;

        public Entity Owner { get; private set; }
        public float HalfWidth => baseHalfWidth * bodyScale;
        public float HalfDepth => baseHalfDepth * bodyScale;
        public float HeightMin => baseHeightMin * bodyScale;
        public float HeightMax => baseHeightMax * bodyScale;
        public bool AcceptsHits => accepts && isActiveAndEnabled && Owner != null && Owner.IsAlive;

        public float WorldX => motor != null ? motor.GroundX : transform.position.x;
        public float WorldZ => motor != null ? motor.GroundZ : transform.position.z;
        public float WorldHeightMin => (motor != null ? motor.JumpHeight : 0f) + HeightMin;
        public float WorldHeightMax => (motor != null ? motor.JumpHeight : 0f) + HeightMax;

        void OnEnable()
        {
            if (!active.Contains(this)) active.Add(this);
        }

        void OnDisable()
        {
            active.Remove(this);
        }

        public void Configure(Entity owner, Motor2D5 body, float halfWidth, float halfDepth, float heightMin, float heightMax)
        {
            Owner = owner;
            motor = body;
            baseHalfWidth = halfWidth;
            baseHalfDepth = halfDepth;
            baseHeightMin = heightMin;
            baseHeightMax = heightMax;
            bodyScale = 1f;
            accepts = true;
        }

        public void SetBodyScale(float scale) => bodyScale = Mathf.Clamp(scale, 0.45f, 1.65f);

        public void SetAcceptsHits(bool value) => accepts = value;
    }
}
