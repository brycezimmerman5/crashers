using UnityEngine;

namespace Crashers.Combat
{
    public sealed class CameraShake : MonoBehaviour
    {
        public static CameraShake Shared { get; private set; }

        Transform cameraTransform;
        Vector3 origin;
        float timeLeft;
        float magnitude;

        void Awake()
        {
            if (Shared != null && Shared != this)
            {
                Destroy(this);
                return;
            }
            Shared = this;
        }

        public void Bind(Transform camera)
        {
            cameraTransform = camera;
            origin = camera != null ? camera.localPosition : Vector3.zero;
        }

        public void Shake(float strength, float duration)
        {
            magnitude = Mathf.Clamp(Mathf.Max(magnitude, strength), 0f, 0.28f);
            timeLeft = Mathf.Clamp(Mathf.Max(timeLeft, duration), 0f, 0.2f);
        }

        void LateUpdate()
        {
            if (cameraTransform == null) return;
            if (timeLeft <= 0f)
            {
                cameraTransform.localPosition = origin;
                magnitude = 0f;
                return;
            }
            timeLeft -= Time.unscaledDeltaTime;
            float damp = Mathf.Clamp01(timeLeft * 10f);
            Vector2 kick = Random.insideUnitCircle * magnitude * damp;
            cameraTransform.localPosition = origin + new Vector3(kick.x, kick.y, 0f);
        }

        void OnDestroy()
        {
            if (cameraTransform != null) cameraTransform.localPosition = origin;
            if (Shared == this) Shared = null;
        }
    }
}
