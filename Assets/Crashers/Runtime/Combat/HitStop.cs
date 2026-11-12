using UnityEngine;

namespace Crashers.Combat
{
    /// <summary>
    /// Brief time-scale dip on a confirmed hit. Punches refresh a fixed window
    /// and always restore scale to 1, so overlapping hits cannot stack the freeze.
    /// </summary>
    public sealed class HitStop : MonoBehaviour
    {
        public static HitStop Shared { get; private set; }

        const float Dip = 0.05f;
        const float Window = 0.05f;

        float remaining;
        bool active;

        void Awake()
        {
            if (Shared != null && Shared != this)
            {
                Destroy(this);
                return;
            }
            Shared = this;
        }

        public void Punch()
        {
            remaining = Window;
            if (active) return;
            active = true;
            Time.timeScale = Dip;
        }

        void Update()
        {
            if (!active) return;
            remaining -= Time.unscaledDeltaTime;
            if (remaining > 0f) return;
            Restore();
        }

        void OnDisable() => Restore();

        void OnDestroy()
        {
            Restore();
            if (Shared == this) Shared = null;
        }

        void Restore()
        {
            if (!active && Mathf.Abs(Time.timeScale - 1f) < 0.001f) return;
            active = false;
            remaining = 0f;
            Time.timeScale = 1f;
        }
    }
}
