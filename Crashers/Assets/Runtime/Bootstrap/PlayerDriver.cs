using Crashers.Characters;
using Crashers.Combat;
using Crashers.Controls;
using Crashers.Magic;
using Crashers.Progression;
using Crashers.Weapons;
using UnityEngine;

namespace Crashers.Bootstrap
{
    public sealed class PlayerDriver : MonoBehaviour
    {
        IInputSource input;
        Motor2D5 motor;
        CombatController combat;
        AbilityCaster caster;
        Entity entity;
        Equipment equipment;
        Inventory inventory;
        Health health;
        Hurtbox hurtbox;
        HitReaction reaction;
        SpriteFlash flash;
        float spawnX;
        float spawnZ;
        float deadTimer = -1f;

        public void Configure(
            IInputSource source,
            Motor2D5 mover,
            CombatController attacks,
            AbilityCaster magic,
            Entity body,
            Equipment gear,
            Inventory bag,
            Health life,
            Hurtbox box,
            HitReaction hits,
            SpriteFlash sprite,
            float x,
            float z)
        {
            input = source;
            motor = mover;
            combat = attacks;
            caster = magic;
            entity = body;
            equipment = gear;
            inventory = bag;
            health = life;
            hurtbox = box;
            reaction = hits;
            flash = sprite;
            spawnX = x;
            spawnZ = z;
            if (health != null) health.Died += OnDied;
            if (reaction != null) reaction.Stunned += OnStunned;
        }

        void OnStunned()
        {
            if (combat != null) combat.Cancel();
        }

        void OnDied(Health _)
        {
            deadTimer = 2f;
            if (combat != null) combat.Cancel();
            if (hurtbox != null) hurtbox.SetAcceptsHits(false);
            if (motor != null)
            {
                motor.SetMoveInput(Vector2.zero);
                motor.SetControlLocked(true);
            }
            if (flash != null) flash.SetDead();
        }

        void Update()
        {
            if (health != null && !health.IsAlive)
            {
                if (deadTimer > 0f) deadTimer -= Time.unscaledDeltaTime;
                if (deadTimer <= 0f && deadTimer > -0.5f) Respawn();
                return;
            }
            if (input == null || motor == null || entity == null) return;

            bool locked = reaction != null && reaction.IsLocked;
            Vector2 move = input.Move;
            if (!locked && move.x > 0.2f) entity.SetFacing(1);
            else if (!locked && move.x < -0.2f) entity.SetFacing(-1);
            motor.SetMoveInput(locked ? Vector2.zero : move);
            if (locked) return;
            if (input.JumpPressed) motor.RequestJump();
            if (combat != null && input.LightPressed) combat.BufferLight();
            if (combat != null && input.HeavyPressed) combat.BufferHeavy();
            if (caster != null && input.Ability1Pressed) caster.TryCastSlot(0);
            if (caster != null && input.Ability2Pressed) caster.TryCastSlot(1);
            if (caster != null && input.Ability3Pressed) caster.TryCastSlot(2);
            if (input.InteractPressed) TryPickup();
        }

        void TryPickup()
        {
            if (motor == null) return;
            WeaponPickup pickup = WeaponPickup.FindClosest(motor.GroundX, motor.GroundZ, 0.95f, 0.75f);
            if (pickup != null) pickup.Give(inventory, equipment);
        }

        void Respawn()
        {
            deadTimer = -1f;
            if (health != null) health.Revive();
            if (hurtbox != null) hurtbox.SetAcceptsHits(true);
            if (reaction != null) reaction.ResetReaction();
            if (motor != null)
            {
                motor.SetControlLocked(false);
                motor.Teleport(spawnX, spawnZ);
            }
            if (flash != null) flash.Restore();
        }
    }
}
