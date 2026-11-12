using UnityEngine;

namespace Crashers.Characters
{
    public sealed class SpriteFlash : MonoBehaviour
    {
        SpriteRenderer[] sprites = System.Array.Empty<SpriteRenderer>();
        Color baseColor = Color.white;
        Color held = Color.white;
        float timer;
        bool holding;

        void Awake()
        {
            SpriteRenderer sprite = GetComponent<SpriteRenderer>();
            if (sprite != null) Adopt(new[] { sprite });
        }

        public void Adopt(SpriteRenderer[] renderers)
        {
            if (renderers == null || renderers.Length == 0) return;
            sprites = renderers;
            for (int i = 0; i < sprites.Length; i++)
            {
                if (sprites[i] == null) continue;
                baseColor = sprites[i].color;
                break;
            }
        }

        public void SetBase(Color color)
        {
            baseColor = color;
            if (timer <= 0f && !holding) Paint(color);
        }

        public void Flash()
        {
            timer = 0.08f;
            Paint(Color.white);
        }

        public void Hold(Color color)
        {
            holding = true;
            held = color;
            if (timer <= 0f) Paint(color);
        }

        public void Release()
        {
            holding = false;
            if (timer <= 0f) Paint(baseColor);
        }

        public void SetDead()
        {
            holding = false;
            timer = 0f;
            Paint(new Color(0.35f, 0.35f, 0.38f, 1f));
        }

        public void Restore()
        {
            holding = false;
            timer = 0f;
            Paint(baseColor);
        }

        void Update()
        {
            if (timer <= 0f) return;
            timer -= Time.unscaledDeltaTime;
            if (timer <= 0f) Paint(holding ? held : baseColor);
        }

        void Paint(Color color)
        {
            for (int i = 0; i < sprites.Length; i++)
            {
                if (sprites[i] != null) sprites[i].color = color;
            }
        }
    }
}
