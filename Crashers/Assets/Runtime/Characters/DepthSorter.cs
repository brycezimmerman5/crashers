using UnityEngine;

namespace Crashers.Characters
{
    /// <summary>
    /// Higher ground Z is farther from the camera and receives a lower sorting order,
    /// so nearer characters draw on top. Jump height is not part of the sort key.
    /// </summary>
    public sealed class DepthSorter : MonoBehaviour
    {
        public int Scale = 100;
        public int Bias;

        SpriteRenderer sprite;
        Motor2D5 motor;

        void Awake()
        {
            sprite = GetComponent<SpriteRenderer>();
            motor = GetComponentInParent<Motor2D5>();
        }

        void LateUpdate()
        {
            if (sprite == null) return;
            float groundZ = motor != null ? motor.GroundZ : transform.position.z;
            sprite.sortingOrder = Bias - Mathf.RoundToInt(groundZ * Scale);
        }
    }
}
