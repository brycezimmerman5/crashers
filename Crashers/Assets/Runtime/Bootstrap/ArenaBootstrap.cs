using System.Collections.Generic;
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
    /// Sections run to the right. The current fight stays inside its X range until it is clear.
    /// </summary>
    public sealed class ArenaBootstrap : MonoBehaviour
    {
        const float DepthNear = 0.9f;
        const float DepthFar = 8.8f;
        const float ArenaLeft = -8f;
        const float SectionWidth = 16f;
        const float PlayerStartX = -3.2f;
        const float PlayerStartZ = 2.8f;

        readonly List<EnemyBrain> sectionEnemies = new List<EnemyBrain>();

        PlayBounds bounds;
        Transform ground;
        Vector2 groundSpriteSize;
        Motor2D5 playerMotor;
        GameContent content;
        ProgressionService progression;
        EnemySlotAssigner slots;
        Actor player;
        Sprite meleeSprite;
        Sprite rangedSprite;
        Sprite bossSprite;
        Sprite swordSprite;
        Sprite shotSprite;
        Sprite areaSprite;
        Sprite barSprite;
        int activeSection;
        int armedMelee;
        bool gateOpen;

        bool gameStarted;

        void Start() => GameIntro.Open(this);

        public void BeginGame()
        {
            if (gameStarted) return;
            gameStarted = true;
            GamePace.Apply();
            Build();
        }

        void OnDestroy()
        {
            Time.timeScale = 1f;
        }

        void Update()
        {
            if (!gateOpen || playerMotor == null || bounds == null) return;
            float nextLeft = ArenaLeft + (activeSection + 1) * SectionWidth;
            if (playerMotor.GroundX < nextLeft + 0.45f) return;
            int next = activeSection + 1;
            SpawnWave(next);
            activeSection = next;
            gateOpen = false;
            bounds.SetHorizontal(ArenaLeft, SectionRight(next));
            LayoutGround();
        }

        void Build()
        {
            content = GameContent.Create();
            Sprite playerSprite = PlaceholderSprites.Character(new Color(0.22f, 0.45f, 0.95f), new Color(0.96f, 0.82f, 0.68f));
            meleeSprite = PlaceholderSprites.Character(new Color(0.82f, 0.18f, 0.16f), new Color(0.35f, 0.16f, 0.14f));
            rangedSprite = PlaceholderSprites.Character(new Color(0.58f, 0.28f, 0.86f), new Color(0.9f, 0.86f, 0.95f));
            bossSprite = PlaceholderSprites.Character(new Color(0.95f, 0.55f, 0.12f), new Color(0.45f, 0.22f, 0.08f));
            swordSprite = PlaceholderSprites.Sword(new Color(0.85f, 0.88f, 0.95f), new Color(0.55f, 0.32f, 0.12f));
            shotSprite = PlaceholderSprites.Blob(Color.white, 12);
            areaSprite = PlaceholderSprites.Blob(Color.white, 24);
            barSprite = PlaceholderSprites.WhiteQuad();

            bounds = CreateBounds();
            CreateGround();
            new GameObject("Hit Stop").AddComponent<HitStop>();
            slots = new GameObject("Enemy Slots").AddComponent<EnemySlotAssigner>();

            player = CreateActor(
                "Player", content.Player, playerSprite, Faction.Player,
                PlayerStartX, PlayerStartZ, bounds, 1f, 0.28f, 0.34f, 0.05f, 1.35f,
                true, true, shotSprite, areaSprite, barSprite, true);
            playerMotor = player.Motor;
            player.Combat.SetLoadout(content.Player.LightChain, content.Player.HeavyChain, content.Player.Aerial);
            CreateCamera(playerMotor);

            progression = CreateProgression(player, content);
            player.Caster.Configure(player.Entity, player.Motor, player.Stats, player.Mana, progression.Unlocks, shotSprite, areaSprite);
            player.Caster.SetSlots(content.Fireball, content.Shockwave, content.Bolt);

            var input = player.Root.AddComponent<PlayerInputSource>();
            var driver = player.Root.AddComponent<PlayerDriver>();
            driver.Configure(
                input, player.Motor, player.Combat, player.Caster, player.Entity,
                player.Equipment, player.Inventory, player.Health, player.Hurtbox,
                player.Reaction, player.Flash, PlayerStartX, PlayerStartZ);

            SpawnEnemy(content.Melee, meleeSprite, 1.8f, 4.7f, 1f, 0.3f, 0.34f, 1.3f);
            SpawnEnemy(content.Ranged, rangedSprite, 5.2f, 7.1f, 1f, 0.26f, 0.3f, 1.25f);
            SpawnEnemy(content.Boss, bossSprite, 6.4f, 2.35f, 1.45f, 0.46f, 0.42f, 1.85f);

            ArenaHud.Build(
                player.Health, progression.Experience, content.Curve, progression.Wallet,
                player.Mana, progression.Unlocks, player.Equipment, player.Motor,
                GameContent.BoltId, GameContent.BoltUnlockLevel);
        }

        static float SectionRight(int index) => ArenaLeft + (index + 1) * SectionWidth;

        PlayBounds CreateBounds()
        {
            var boundsObject = new GameObject("Play Bounds");
            var playBounds = boundsObject.AddComponent<PlayBounds>();
            playBounds.Set(ArenaLeft, SectionRight(0), DepthNear, DepthFar);
            return playBounds;
        }

        static void CreateCamera(Motor2D5 target)
        {
            var rig = new GameObject("Camera Rig");
            rig.transform.position = new Vector3(target != null ? target.GroundX : 0f, 0f, 0f);
            var follow = rig.AddComponent<CameraFollow>();
            follow.Bind(target);

            var cameraObject = new GameObject("Arena Camera");
            cameraObject.transform.SetParent(rig.transform, false);
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 4.85f;
            camera.transform.localPosition = new Vector3(0f, 2.9f, -12f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.09f, 0.13f);
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 50f;
            cameraObject.AddComponent<AudioListener>();
            var shake = cameraObject.AddComponent<CameraShake>();
            shake.Bind(camera.transform);
        }

        void CreateGround()
        {
            var groundObject = new GameObject("Ground");
            var renderer = groundObject.AddComponent<SpriteRenderer>();
            renderer.sprite = PlaceholderSprites.Gradient(
                new Color(0.11f, 0.16f, 0.1f),
                new Color(0.24f, 0.36f, 0.2f),
                4, 16);
            renderer.sortingOrder = -5000;
            ground = groundObject.transform;
            groundSpriteSize = renderer.sprite.bounds.size;
            LayoutGround();
        }

        void LayoutGround()
        {
            if (ground == null || bounds == null || groundSpriteSize.x < 0.001f || groundSpriteSize.y < 0.001f) return;
            float yMin = DepthNear * Motor2D5.DepthToScreenY - 0.5f;
            float yMax = DepthFar * Motor2D5.DepthToScreenY + 0.95f;
            float xMin = ArenaLeft - 2f;
            float xMax = bounds.XMax + 14f;
            float width = Mathf.Max(1f, xMax - xMin);
            float height = Mathf.Max(1f, yMax - yMin);
            ground.position = new Vector3((xMin + xMax) * 0.5f, (yMin + yMax) * 0.5f, 12f);
            ground.localScale = new Vector3(width / groundSpriteSize.x, height / groundSpriteSize.y, 1f);
        }

        void SpawnWave(int index)
        {
            float left = ArenaLeft + index * SectionWidth;
            float zNear = Mathf.Lerp(DepthNear, DepthFar, 0.28f);
            float zMid = Mathf.Lerp(DepthNear, DepthFar, 0.55f);
            float zFar = Mathf.Lerp(DepthNear, DepthFar, 0.8f);
            int mix = index % 3;
            if (mix == 1)
            {
                SpawnEnemy(content.Melee, meleeSprite, left + 5.6f, zMid, 1f, 0.3f, 0.34f, 1.3f);
                SpawnEnemy(content.Melee, meleeSprite, left + 9f, zNear, 1f, 0.3f, 0.34f, 1.3f);
                SpawnEnemy(content.Ranged, rangedSprite, left + 12.4f, zFar, 1f, 0.26f, 0.3f, 1.25f);
            }
            else if (mix == 2)
            {
                SpawnEnemy(content.Ranged, rangedSprite, left + 6.2f, zFar, 1f, 0.26f, 0.3f, 1.25f);
                SpawnEnemy(content.Melee, meleeSprite, left + 9.3f, zMid, 1f, 0.3f, 0.34f, 1.3f);
                SpawnEnemy(content.Boss, bossSprite, left + 12.6f, zNear, 1.45f, 0.46f, 0.42f, 1.85f);
            }
            else
            {
                SpawnEnemy(content.Melee, meleeSprite, left + 5.4f, zMid, 1f, 0.3f, 0.34f, 1.3f);
                SpawnEnemy(content.Ranged, rangedSprite, left + 9.1f, zFar, 1f, 0.26f, 0.3f, 1.25f);
                SpawnEnemy(content.Boss, bossSprite, left + 12.5f, zNear, 1.45f, 0.46f, 0.42f, 1.85f);
            }
        }

        void OpenGate()
        {
            if (gateOpen || bounds == null) return;
            gateOpen = true;
            bounds.SetHorizontal(ArenaLeft, SectionRight(activeSection + 1));
            LayoutGround();
        }

        void SpawnEnemy(
            EnemyArchetype archetype,
            Sprite sprite,
            float x,
            float z,
            float scale,
            float halfWidth,
            float halfDepth,
            float heightMax)
        {
            if (archetype == null) return;
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
            enemy.Reaction.SetKnockdownHits(3);
            sectionEnemies.Add(brain);
            brain.Killed += OnEnemyKilled;
            ArmEnemy(enemy, archetype);
        }

        void ArmEnemy(Actor enemy, EnemyArchetype archetype)
        {
            if (enemy == null || enemy.Root == null) return;
            var gear = enemy.Root.AddComponent<Equipment>();
            gear.Configure(null, enemy.Stats, enemy.Combat);
            if (archetype.Role == EnemyRole.Ranged)
            {
                gear.PresentSword(PlaceholderSprites.Staff(), Color.white, 1.05f);
                gear.BindCaster(enemy.Caster);
                return;
            }
            bool sword = archetype.Role == EnemyRole.Boss || (armedMelee++ % 2) == 1;
            if (!sword)
            {
                gear.PresentFists();
                return;
            }
            float size = archetype.Role == EnemyRole.Boss ? 1.25f : 1f;
            gear.PresentSword(swordSprite, new Color(0.86f, 0.9f, 0.95f), size);
            if (enemy.Caster != null) gear.BindCaster(enemy.Caster);
        }

        void OnEnemyKilled(EnemyBrain brain)
        {
            sectionEnemies.Remove(brain);
            if (brain != null && brain.Archetype != null && progression != null) progression.RegisterKill(brain.Archetype);
            if (brain != null && brain.Archetype != null && brain.Archetype.WeaponDropId == GameContent.SwordId && content != null && content.Sword != null)
            {
                var pickupObject = new GameObject("Broadsword Pickup");
                var pickup = pickupObject.AddComponent<WeaponPickup>();
                pickup.Spawn(content.Sword, brain.Motor.GroundX, brain.Motor.GroundZ, swordSprite, new Color(1f, 0.86f, 0.25f));
            }
            if (sectionEnemies.Count == 0) OpenGate();
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
            Sprite barSprite,
            bool useViking = false)
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

            var body = new GameObject("Body");
            body.transform.SetParent(actor.Root.transform, false);
            var visual = new GameObject("Visual");
            visual.transform.SetParent(body.transform, false);
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
            var depth = actor.Root.AddComponent<DepthScale>();
            depth.Bind(actor.Motor, bounds, actor.Hurtbox);
            actor.Reaction.Configure(actor.Health, actor.Motor, actor.Flash, actor.Hurtbox);
            actor.Combat.Configure(actor.Entity, actor.Motor, actor.Stats, actor.Flash);
            if (definition != null) actor.Combat.SetLoadout(definition.LightChain, definition.HeavyChain, definition.Aerial);
            if (actor.Mana != null)
            {
                actor.Mana.Bind(actor.Stats);
                actor.Caster.Configure(actor.Entity, actor.Motor, actor.Stats, actor.Mana, null, projectileSprite, areaSprite);
            }
            if (actor.Equipment != null) actor.Equipment.Configure(actor.Inventory, actor.Stats, actor.Combat);
            SpriteRenderer sortBody = renderer;
            float barWidth = 0.9f * visualScale;
            if (useViking && VikingRig.TryMount(
                    visual.transform, actor.Entity, actor.Combat, actor.Motor, actor.Hurtbox, actor.Flash,
                    out SpriteRenderer vikingBody, out float vikingWidth))
            {
                renderer.enabled = false;
                FacingSprite facing = visual.GetComponent<FacingSprite>();
                if (facing != null) facing.enabled = false;
                if (vikingBody != null) sortBody = vikingBody;
                barWidth = vikingWidth;
                barObject.transform.localPosition = new Vector3(0f, VikingArt.TargetHeight + 0.02f, 0f);
            }
            bars.Bind(actor.Health, bar, sortBody, barWidth);
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
