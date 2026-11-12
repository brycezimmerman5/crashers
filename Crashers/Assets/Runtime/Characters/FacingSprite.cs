using UnityEngine;

namespace Crashers.Characters
{
    public sealed class FacingSprite : MonoBehaviour
    {
        SpriteRenderer sprite;
        Entity entity;

        void Awake()
        {
            sprite = GetComponent<SpriteRenderer>();
            entity = GetComponentInParent<Entity>();
        }

        void LateUpdate()
        {
            if (sprite == null || entity == null) return;
            sprite.flipX = entity.Facing < 0;
        }
    }
}
