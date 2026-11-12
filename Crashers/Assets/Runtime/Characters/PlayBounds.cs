using UnityEngine;

namespace Crashers.Characters
{
    /// <summary>Invisible arena floor in ground X/Z. Y is not a bound; jump height is separate.</summary>
    public sealed class PlayBounds : MonoBehaviour
    {
        public float XMin = -8f;
        public float XMax = 8f;
        public float ZMin = 0.9f;
        public float ZMax = 8.8f;

        public void Set(float xMin, float xMax, float zMin, float zMax)
        {
            XMin = xMin;
            XMax = xMax;
            ZMin = zMin;
            ZMax = zMax;
        }

        public void SetHorizontal(float xMin, float xMax)
        {
            XMin = xMin;
            XMax = Mathf.Max(xMin + 1f, xMax);
        }

        public bool Contains(float x, float z)
        {
            return x >= XMin && x <= XMax && z >= ZMin && z <= ZMax;
        }

        public void Clamp(ref float x, ref float z)
        {
            if (x < XMin) x = XMin;
            if (x > XMax) x = XMax;
            if (z < ZMin) z = ZMin;
            if (z > ZMax) z = ZMax;
        }
    }
}
