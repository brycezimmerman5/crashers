using Crashers.Characters;
using UnityEngine;

namespace Crashers.Bootstrap
{
    /// <summary>
    /// Moves the camera rig on X so the view stays with the player.
    /// The camera child keeps its own height, and shake stays on that child.
    /// </summary>
    public sealed class CameraFollow : MonoBehaviour
    {
        Motor2D5 target;

        public void Bind(Motor2D5 motor) => target = motor;

        void LateUpdate()
        {
            if (target == null) return;
            Vector3 position = transform.position;
            position.x = target.GroundX;
            transform.position = position;
        }
    }
}
