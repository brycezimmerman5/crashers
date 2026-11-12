using UnityEngine;

namespace Crashers.Combat
{
    /// <summary>
    /// Slightly slower gameplay. Hit-stop restores this scale instead of full speed.
    /// </summary>
    public static class GamePace
    {
        public const float Scale = 0.85f;

        public static void Apply() => Time.timeScale = Scale;
    }
}
