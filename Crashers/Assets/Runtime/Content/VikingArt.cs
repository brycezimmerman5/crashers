using UnityEngine;

namespace Crashers.Content
{
    /// <summary>
    /// Imported viking PSD and its animator, filled by the editor loader before Play.
    /// </summary>
    public static class VikingArt
    {
        public const string AttackTrigger = "attack1";
        public const string IdleState = "idle";
        public const float PixelsPerUnit = 100f;
        public const float DocumentWidth = 1536f;
        public const float TargetHeight = 1.65f;

        public static GameObject Prefab { get; set; }
        public static RuntimeAnimatorController Controller { get; set; }
        public static Sprite Head { get; set; }
        public static Sprite Torso { get; set; }
        public static Sprite LeftHand { get; set; }
        public static Sprite RightHand { get; set; }
        public static Sprite LeftFoot { get; set; }
        public static Sprite RightFoot { get; set; }

        public static bool HasController => Controller != null;

        public static bool HasSprites =>
            Head != null && Torso != null && LeftHand != null && RightHand != null && LeftFoot != null && RightFoot != null;
    }
}
