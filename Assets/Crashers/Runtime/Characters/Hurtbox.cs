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

        public Entity Owner { get; private set; }
        public float HalfWidth { get; private set; } = 0.28f;
        public float HalfDepth { get; private set; } = 0.32f;
        public float HeightMin { get; private set; } = 0.05f;
        public float HeightMax { get; private set; } = 1.35f;
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
            HalfWidth = halfWidth;
            HalfDepth = halfDepth;
            HeightMin = heightMin;
            HeightMax = heightMax;
            accepts = true;
        }

        public void SetAcceptsHits(bool value) => accepts = value;
    }
}
