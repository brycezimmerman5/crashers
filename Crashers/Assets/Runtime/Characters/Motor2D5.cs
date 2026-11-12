using UnityEngine;

namespace Crashers.Characters
{
    /// <summary>
    /// Kinematic 2.5D motor for players and enemies.
    /// Ground motion is X and Z. GroundZ is arena depth, not camera depth.
    /// The camera sits on -Z looking toward +Z, so a larger GroundZ is farther away.
    /// Transform.position.y is visual height only:
    ///   x = GroundX
    ///   y = JumpHeight + GroundZ * DepthToScreenY
    ///   z = GroundZ
    /// Walking up the screen increases GroundZ. DepthSorter reads GroundZ and ignores JumpHeight.
    /// </summary>
    public sealed class Motor2D5 : MonoBehaviour
    {
        public const float DepthToScreenY = 0.65f;

        public float Gravity = 32f;
        public float JumpSpeed = 9.4f;
        public float CoyoteTime = 0.12f;
        public float GroundSnap = 0.05f;
        public float KnockbackDecay = 10f;

        float groundX;
        float groundZ;
        float jumpHeight;
        float verticalVelocity;
        float moveSpeed = 5.2f;
        float knockX;
        float knockZ;
        float coyote;
        float jumpBuffer;
        Vector2 wish;
        bool jumpedThisAir;
        PlayBounds bounds;

        public float GroundX => groundX;
        public float GroundZ => groundZ;
        public float JumpHeight => jumpHeight;
        public float VerticalVelocity => verticalVelocity;
        public bool IsGrounded { get; private set; } = true;
        public bool ControlLocked { get; private set; }
        public bool Launched { get; private set; }

        public void Configure(PlayBounds playBounds, float speed)
        {
            bounds = playBounds;
            moveSpeed = Mathf.Max(0.5f, speed);
        }

        public void SetMoveSpeed(float speed) => moveSpeed = Mathf.Max(0.5f, speed);

        public void Teleport(float x, float z)
        {
            groundX = x;
            groundZ = z;
            if (bounds != null) bounds.Clamp(ref groundX, ref groundZ);
            jumpHeight = 0f;
            verticalVelocity = 0f;
            knockX = 0f;
            knockZ = 0f;
            wish = Vector2.zero;
            Launched = false;
            jumpedThisAir = false;
            IsGrounded = true;
            coyote = CoyoteTime;
            ApplyPose();
        }

        public void SetMoveInput(Vector2 planar) => wish = Vector2.ClampMagnitude(planar, 1f);

        public void RequestJump() => jumpBuffer = 0.12f;

        public void SetControlLocked(bool locked) => ControlLocked = locked;

        public void ApplyImpulse(float x, float z, float launchY)
        {
            knockX += x;
            knockZ += z;
            if (launchY > 0.01f)
            {
                verticalVelocity = launchY;
                if (jumpHeight < 0.06f) jumpHeight = 0.06f;
                IsGrounded = false;
                Launched = true;
                coyote = 0f;
                jumpedThisAir = true;
            }
        }

        public void ClearMotion()
        {
            wish = Vector2.zero;
            knockX = 0f;
            knockZ = 0f;
            verticalVelocity = 0f;
            jumpHeight = 0f;
            Launched = false;
            jumpedThisAir = false;
            IsGrounded = true;
            coyote = CoyoteTime;
            ApplyPose();
        }

        void Update() => Simulate(Time.deltaTime);

        public void Simulate(float dt)
        {
            if (dt < 0f) dt = 0f;
            bool supported = bounds == null || bounds.Contains(groundX, groundZ);
            bool onFloor = supported && jumpHeight <= GroundSnap && verticalVelocity <= 0.05f;

            if (onFloor)
            {
                IsGrounded = true;
                jumpHeight = 0f;
                verticalVelocity = 0f;
                coyote = CoyoteTime;
                jumpedThisAir = false;
                Launched = false;
            }
            else
            {
                if (IsGrounded && !jumpedThisAir) coyote = CoyoteTime;
                IsGrounded = false;
                coyote -= dt;
                verticalVelocity -= Gravity * dt;
                jumpHeight += verticalVelocity * dt;
                if (jumpHeight <= 0f)
                {
                    jumpHeight = 0f;
                    verticalVelocity = 0f;
                    if (supported)
                    {
                        IsGrounded = true;
                        coyote = CoyoteTime;
                        jumpedThisAir = false;
                        Launched = false;
                    }
                }
            }

            if (jumpBuffer > 0f && !ControlLocked && (IsGrounded || coyote > 0f))
            {
                verticalVelocity = JumpSpeed;
                IsGrounded = false;
                if (jumpHeight < 0.02f) jumpHeight = 0.02f;
                coyote = 0f;
                jumpedThisAir = true;
                Launched = false;
                jumpBuffer = 0f;
            }
            else if (jumpBuffer > 0f)
            {
                jumpBuffer -= dt;
            }

            float inputX = ControlLocked ? 0f : wish.x * moveSpeed;
            float inputZ = ControlLocked ? 0f : wish.y * moveSpeed;
            knockX = Mathf.MoveTowards(knockX, 0f, KnockbackDecay * dt);
            knockZ = Mathf.MoveTowards(knockZ, 0f, KnockbackDecay * dt);
            groundX += (inputX + knockX) * dt;
            groundZ += (inputZ + knockZ) * dt;
            if (bounds != null) bounds.Clamp(ref groundX, ref groundZ);
            ApplyPose();
        }

        void ApplyPose()
        {
            transform.position = new Vector3(groundX, jumpHeight + groundZ * DepthToScreenY, groundZ);
        }

        public static void Place(Transform target, float x, float z, float height)
        {
            if (target == null) return;
            target.position = new Vector3(x, height + z * DepthToScreenY, z);
        }
    }
}
