using UnityEngine;

namespace Crashers.Characters
{
    /// <summary>Invisible arena floor in ground X/Z. Y is not a bound; jump height is separate.</summary>
    public sealed class PlayBounds : MonoBehaviour
    {
        public float XMin = -7.6f;
        public float XMax = 7.6f;
        public float ZMin = 0.35f;
        public float ZMax = 5.05f;

        public void Set(float xMin, float xMax, float zMin, float zMax)
        {
            XMin = xMin;
            XMax = xMax;
            ZMin = zMin;
            ZMax = zMax;
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
