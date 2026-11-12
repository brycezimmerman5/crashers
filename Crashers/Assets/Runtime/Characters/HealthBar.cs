using UnityEngine;

namespace Crashers.Characters
{
    public sealed class HealthBar : MonoBehaviour
    {
        const float BarHeight = 0.1f;
        const float WidthScale = 1.05f;

        Health health;
        SpriteRenderer fill;
        SpriteRenderer body;
        float width = 1.8f;
        bool placed;

        public void Bind(Health source, SpriteRenderer bar, SpriteRenderer bodySprite, float barWidth)
        {
            health = source;
            fill = bar;
            body = bodySprite;
            width = barWidth * WidthScale;
            if (placed || fill == null) return;
            placed = true;
            Vector3 pos = fill.transform.localPosition;
            pos.x = -width * 0.5f;
            pos.y += 0.22f;
            fill.transform.localPosition = pos;
        }

        void LateUpdate()
        {
            transform.rotation = Quaternion.identity;
            if (health == null || fill == null) return;
            bool show = health.IsAlive && health.Fraction < 0.999f;
            fill.enabled = show;
            if (!show) return;
            float amount = Mathf.Clamp01(health.Fraction);
            var scale = fill.transform.localScale;
            scale.x = Mathf.Max(0.05f, width * amount);
            scale.y = BarHeight;
            fill.transform.localScale = scale;
            fill.color = Color.Lerp(new Color(0.85f, 0.15f, 0.12f), new Color(0.25f, 0.85f, 0.28f), amount);
            if (body != null) fill.sortingOrder = body.sortingOrder + 2;
        }
    }
}
