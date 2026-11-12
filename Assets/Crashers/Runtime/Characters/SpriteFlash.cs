using UnityEngine;

namespace Crashers.Characters
{
    public sealed class SpriteFlash : MonoBehaviour
    {
        SpriteRenderer sprite;
        Color baseColor = Color.white;
        Color held = Color.white;
        float timer;
        bool holding;

        void Awake()
        {
            sprite = GetComponent<SpriteRenderer>();
            if (sprite != null) baseColor = sprite.color;
        }

        public void SetBase(Color color)
        {
            baseColor = color;
            if (timer <= 0f && !holding && sprite != null) sprite.color = color;
        }

        public void Flash()
        {
            timer = 0.08f;
            if (sprite != null) sprite.color = Color.white;
        }

        public void Hold(Color color)
        {
            holding = true;
            held = color;
            if (timer <= 0f && sprite != null) sprite.color = color;
        }

        public void Release()
        {
            holding = false;
            if (timer <= 0f && sprite != null) sprite.color = baseColor;
        }

        public void SetDead()
        {
            holding = false;
            timer = 0f;
            if (sprite != null) sprite.color = new Color(0.35f, 0.35f, 0.38f, 1f);
        }

        public void Restore()
        {
            holding = false;
            timer = 0f;
            if (sprite != null) sprite.color = baseColor;
        }

        void Update()
        {
            if (timer <= 0f || sprite == null) return;
            timer -= Time.unscaledDeltaTime;
            if (timer <= 0f) sprite.color = holding ? held : baseColor;
        }
    }
}
