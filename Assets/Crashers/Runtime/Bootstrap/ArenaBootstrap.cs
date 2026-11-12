using Crashers.Characters;
using Crashers.Combat;
using Crashers.Content;
using Crashers.Controls;
using Crashers.Enemies;
using Crashers.Magic;
using Crashers.Progression;
using Crashers.Ui;
using Crashers.Weapons;
using UnityEngine;

namespace Crashers.Bootstrap
{
    /// <summary>
    /// Builds the test arena at runtime. The scene itself only references this component.
    /// </summary>
    public sealed class ArenaBootstrap : MonoBehaviour
    {
        void Start() => Build();

        void Build()
        {
            GameContent content = GameContent.Create();
            Sprite playerSprite = PlaceholderSprites.Character(new Color(0.22f, 0.45f, 0.95f), new Color(0.96f, 0.82f, 0.68f));
            Sprite meleeSprite = PlaceholderSprites.Character(new Color(0.82f, 0.18f, 0.16f), new Color(0.35f, 0.16f, 0.14f));
            Sprite rangedSprite = PlaceholderSprites.Character(new Color(0.58f, 0.28f, 0.86f), new Color(0.9f, 0.86f, 0.95f));
            Sprite bossSprite = PlaceholderSprites.Character(new Color(0.95f, 0.55f, 0.12f), new Color(0.45f, 0.22f, 0.08f));
            Sprite swordSprite = PlaceholderSprites.Sword(new Color(0.85f, 0.88f, 0.95f), new Color(0.55f, 0.32f, 0.12f));
            Sprite shotSprite = PlaceholderSprites.Blob(Color.white, 12);
            Sprite areaSprite = PlaceholderSprites.Blob(Color.white, 24);
            Sprite barSprite = PlaceholderSprites.WhiteQuad();

            PlayBounds bounds = CreateBounds();
            CreateCamera();
            CreateGround();
            new GameObject("Hit Stop").AddComponent<HitStop>();
            var slots = new GameObject("Enemy Slots").AddComponent<EnemySlotAssigner>();

            Actor player = CreateActor(
                "Player", content.Player, playerSprite, Faction.Player,
                -3.2f, 2.25f, bounds, 1f, 0.28f, 0.34f, 0.05f, 1.35f,
                true, true, shotSprite, areaSprite, barSprite);
            player.Combat.SetLoadout(content.Player.LightChain, content.Player.HeavyChain, content.Player.Aerial);

            ProgressionService progression = CreateProgression(player, content);
            player.Caster.Configure(player.Entity, player.Motor, player.Stats, player.Mana, progression.Unlocks, shotSprite, areaSprite);
            player.Caster.SetSlots(content.Fireball, content.Shockwave, content.Bolt);

            var input = player.Root.AddComponent<PlayerInputSource>();
            var driver = player.Root.AddComponent<PlayerDriver>();
            driver.Configure(
                input, player.Motor, player.Combat, player.Caster, player.Entity,
                player.Equipment, player.Inventory, player.Health, player.Hurtbox,
                player.Reaction, player.Flash, -3.2f, 2.25f);

            SpawnEnemy(content.Melee, meleeSprite, 1.7f, 2.05f, bounds, slots, player, progression, content, swordSprite, shotSprite, areaSprite, barSprite, 1f, 0.3f, 0.34f, 1.3f);
            SpawnEnemy(content.Ranged, rangedSprite, 5.3f, 3.45f, bounds, slots, player, progression, content, swordSprite, shotSprite, areaSprite, barSprite, 1f, 0.26f, 0.3f, 1.25f);
            SpawnEnemy(content.Boss, bossSprite, 6.3f, 1.15f, bounds, slots, player, progression, content, swordSprite, shotSprite, areaSprite, barSprite, 1.45f, 0.46f, 0.42f, 1.85f);

            ArenaHud.Build(
                player.Health, progression.Experience, content.Curve, progression.Wallet,
                player.Mana, progression.Unlocks, player.Equipment, player.Motor,
                GameContent.BoltId, GameContent.BoltUnlockLevel);
        }

        static PlayBounds CreateBounds()
        {
            var boundsObject = new GameObject("Play Bounds");
            var bounds = boundsObject.AddComponent<PlayBounds>();
            bounds.Set(-7.7f, 7.7f, 0.35f, 5.05f);
            return bounds;
        }

        static void CreateCamera()
        {
            var cameraObject = new GameObject("Arena Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 4.85f;
            camera.transform.position = new Vector3(0f, 2.35f, -12f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.09f, 0.13f);
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 50f;
            cameraObject.AddComponent<AudioListener>();
            var shake = cameraObject.AddComponent<CameraShake>();
            shake.Bind(camera.transform);
        }

        static void CreateGround()
        {
            float yMin = 0.35f * Motor2D5.DepthToScreenY - 0.45f;
            float yMax = 5.05f * Motor2D5.DepthToScreenY + 0.6f;
            var ground = new GameObject("Ground");
            var renderer = ground.AddComponent<SpriteRenderer>();
            renderer.sprite = PlaceholderSprites.Gradient(
                new Color(0.11f, 0.16f, 0.1f),
                new Color(0.24f, 0.36f, 0.2f),
                4, 16);
            renderer.sortingOrder = -800;
            Vector2 size = renderer.sprite.bounds.size;
            float height = yMax - yMin;
            ground.transform.position = new Vector3(0f, (yMin + yMax) * 0.5f, 12f);
            ground.transform.localScale = new Vector3(18f / size.x, height / size.y, 1f);
        }

        void SpawnEnemy(
            EnemyArchetype archetype,
            Sprite sprite,
            float x,
            float z,
            PlayBounds bounds,
            EnemySlotAssigner slots,
            Actor player,
            ProgressionService progression,
            GameContent content,
            Sprite swordSprite,
            Sprite shotSprite,
            Sprite areaSprite,
            Sprite barSprite,
            float scale,
            float halfWidth,
            float halfDepth,
            float heightMax)
        {
            bool casts = archetype.RangedAbility != null;
            Actor enemy = CreateActor(
                archetype.DisplayName, archetype.Stats, sprite, Faction.Enemy,
                x, z, bounds, scale, halfWidth, halfDepth, 0.05f, heightMax,
                casts, false, shotSprite, areaSprite, barSprite);
            enemy.Combat.SetLoadout(archetype.LightChain, archetype.HeavyChain, archetype.Aerial);
            if (enemy.Caster != null) enemy.Caster.SetSlots(archetype.RangedAbility, null, null);

            int slot = archetype.Role == EnemyRole.Ranged ? -1 : slots.TakeSlot();
            var brain = enemy.Root.AddComponent<EnemyBrain>();
            brain.Configure(
                archetype, enemy.Entity, player.Entity, enemy.Motor, player.Motor,
                enemy.Combat, enemy.Caster, enemy.Health, enemy.Reaction, enemy.Hurtbox,
                enemy.Flash, enemy.Visual, slots, slot);
            brain.Killed += killed => OnEnemyKilled(killed, progression, content, swordSprite);
        }

        static void OnEnemyKilled(EnemyBrain brain, ProgressionService progression, GameContent content, Sprite swordSprite)
        {
            if (brain.Archetype != null) progression.RegisterKill(brain.Archetype);
            if (brain.Archetype == null || brain.Archetype.WeaponDropId != GameContent.SwordId || content.Sword == null) return;
            var pickupObject = new GameObject("Broadsword Pickup");
            var pickup = pickupObject.AddComponent<WeaponPickup>();
            pickup.Spawn(content.Sword, brain.Motor.GroundX, brain.Motor.GroundZ, swordSprite, new Color(1f, 0.86f, 0.25f));
        }

        static ProgressionService CreateProgression(Actor player, GameContent content)
        {
            var host = new GameObject("Progression");
            var experience = host.AddComponent<Experience>();
            var wallet = host.AddComponent<Wallet>();
            var unlocks = host.AddComponent<Unlocks>();
            var service = host.AddComponent<ProgressionService>();
            service.Configure(
                experience, wallet, unlocks, player.Stats, content.Curve,
                GameContent.FireballId, GameContent.ShockwaveId, GameContent.BoltId, GameContent.BoltUnlockLevel);
            player.Stats.Changed += () =>
            {
                player.Health.RefreshFromStats(true);
                if (player.Mana != null) player.Mana.RefreshFromStats(true);
                player.Motor.SetMoveSpeed(player.Stats.MoveSpeed);
            };
            service.LoadOrNew();
            return service;
        }

        static Actor CreateActor(
            string actorName,
            CharacterDefinition definition,
            Sprite sprite,
            Faction faction,
            float x,
            float z,
            PlayBounds bounds,
            float visualScale,
            float halfWidth,
            float halfDepth,
            float heightMin,
            float heightMax,
            bool withMagic,
            bool withGear,
            Sprite projectileSprite,
            Sprite areaSprite,
            Sprite barSprite)
        {
            var actor = new Actor();
            actor.Root = new GameObject(actorName);
            actor.Entity = actor.Root.AddComponent<Entity>();
            actor.Motor = actor.Root.AddComponent<Motor2D5>();
            actor.Stats = actor.Root.AddComponent<CharacterStats>();
            actor.Health = actor.Root.AddComponent<Health>();
            actor.Reaction = actor.Root.AddComponent<HitReaction>();
            actor.Hurtbox = actor.Root.AddComponent<Hurtbox>();
            actor.Combat = actor.Root.AddComponent<CombatController>();
            if (withMagic)
            {
                actor.Mana = actor.Root.AddComponent<Mana>();
                actor.Caster = actor.Root.AddComponent<AbilityCaster>();
            }
            if (withGear)
            {
                actor.Inventory = actor.Root.AddComponent<Inventory>();
                actor.Equipment = actor.Root.AddComponent<Equipment>();
            }

            var visual = new GameObject("Visual");
            visual.transform.SetParent(actor.Root.transform, false);
            visual.transform.localScale = new Vector3(visualScale, visualScale, 1f);
            actor.Visual = visual.transform;
            var renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            actor.Flash = visual.AddComponent<SpriteFlash>();
            visual.AddComponent<FacingSprite>();
            visual.AddComponent<DepthSorter>();

            var barObject = new GameObject("Health Bar");
            barObject.transform.SetParent(actor.Root.transform, false);
            barObject.transform.localPosition = new Vector3(-0.45f * visualScale, 1.7f * visualScale, 0f);
            var bar = barObject.AddComponent<SpriteRenderer>();
            bar.sprite = barSprite;
            var bars = barObject.AddComponent<HealthBar>();

            actor.Stats.Configure(definition);
            actor.Entity.Configure(faction, actor.Health);
            actor.Health.Bind(actor.Stats);
            actor.Motor.Configure(bounds, actor.Stats.MoveSpeed);
            actor.Motor.Teleport(x, z);
            actor.Hurtbox.Configure(actor.Entity, actor.Motor, halfWidth, halfDepth, heightMin, heightMax);
            actor.Reaction.Configure(actor.Health, actor.Motor, actor.Flash);
            actor.Combat.Configure(actor.Entity, actor.Motor, actor.Stats, actor.Flash);
            if (definition != null) actor.Combat.SetLoadout(definition.LightChain, definition.HeavyChain, definition.Aerial);
            if (actor.Mana != null)
            {
                actor.Mana.Bind(actor.Stats);
                actor.Caster.Configure(actor.Entity, actor.Motor, actor.Stats, actor.Mana, null, projectileSprite, areaSprite);
            }
            if (actor.Equipment != null) actor.Equipment.Configure(actor.Inventory, actor.Stats, actor.Combat);
            bars.Bind(actor.Health, bar, renderer, 0.9f * visualScale);
            return actor;
        }

        sealed class Actor
        {
            public GameObject Root;
            public Entity Entity;
            public Motor2D5 Motor;
            public CharacterStats Stats;
            public Health Health;
            public Mana Mana;
            public HitReaction Reaction;
            public Hurtbox Hurtbox;
            public CombatController Combat;
            public AbilityCaster Caster;
            public SpriteFlash Flash;
            public Transform Visual;
            public Inventory Inventory;
            public Equipment Equipment;
        }
    }
}
