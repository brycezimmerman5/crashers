using UnityEngine;

namespace Crashers.Characters
{
    /// <summary>
    /// Linear screen scale from the playable depth range.
    /// Low ground Z is the near edge (larger). High ground Z is farther up the screen (smaller).
    /// Child fists, swords, and health bars inherit this scale from the body root.
    /// </summary>
    public sealed class DepthScale : MonoBehaviour
    {
        public const float Near = 1.22f;
        public const float Far = 0.7f;

        Motor2D5 motor;
        PlayBounds bounds;
        Hurtbox hurtbox;
        Vector3 authoredScale = Vector3.one;
        bool faceWithEntity;

        public void Bind(Motor2D5 body, PlayBounds playBounds, Hurtbox box, bool face = false)
        {
            motor = body;
            bounds = playBounds;
            hurtbox = box;
            faceWithEntity = face;
            Vector3 scale = transform.localScale;
            authoredScale = new Vector3(
                Mathf.Max(0.01f, Mathf.Abs(scale.x)),
                Mathf.Max(0.01f, Mathf.Abs(scale.y)),
                1f);
        }

        public static float Multiplier(float groundZ, float zMin, float zMax)
        {
            float span = Mathf.Max(0.01f, zMax - zMin);
            float t = Mathf.Clamp01((groundZ - zMin) / span);
            return Mathf.Lerp(Near, Far, t);
        }

        void LateUpdate()
        {
            if (motor == null || bounds == null) return;
            float mul = Multiplier(motor.GroundZ, bounds.ZMin, bounds.ZMax);
            float sign = 1f;
            if (faceWithEntity)
            {
                Entity entity = GetComponent<Entity>();
                if (entity == null) entity = GetComponentInParent<Entity>();
                if (entity != null && entity.Facing < 0) sign = -1f;
            }
            transform.localScale = new Vector3(sign * authoredScale.x * mul, authoredScale.y * mul, 1f);
            if (hurtbox != null) hurtbox.SetBodyScale(mul);
        }
    }
}
