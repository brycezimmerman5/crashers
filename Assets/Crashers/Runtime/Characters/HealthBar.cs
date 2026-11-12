using UnityEngine;

namespace Crashers.Characters
{
    public sealed class HealthBar : MonoBehaviour
    {
        Health health;
        SpriteRenderer fill;
        SpriteRenderer body;
        float width = 0.9f;

        public void Bind(Health source, SpriteRenderer bar, SpriteRenderer bodySprite, float barWidth)
        {
            health = source;
            fill = bar;
            body = bodySprite;
            width = barWidth;
        }

        void LateUpdate()
        {
            if (health == null || fill == null) return;
            bool show = health.IsAlive && health.Fraction < 0.999f;
            fill.enabled = show;
            if (!show) return;
            float amount = Mathf.Clamp01(health.Fraction);
            var scale = fill.transform.localScale;
            scale.x = Mathf.Max(0.05f, width * amount);
            fill.transform.localScale = scale;
            fill.color = Color.Lerp(new Color(0.85f, 0.15f, 0.12f), new Color(0.25f, 0.85f, 0.28f), amount);
            if (body != null) fill.sortingOrder = body.sortingOrder + 2;
        }
    }
}
