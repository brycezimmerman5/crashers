using System.Collections.Generic;
using Crashers.Characters;
using Crashers.Combat;
using Crashers.Enemies;
using Crashers.Magic;
using Crashers.Progression;
using Crashers.Weapons;
using UnityEngine;

namespace Crashers.Content
{
    public sealed class GameContent
    {
        public const int BoltUnlockLevel = 2;
        public const string FireballId = "fireball";
        public const string ShockwaveId = "shockwave";
        public const string BoltId = "bolt";
        public const string SwordId = "broadsword";

        public CharacterDefinition Player;
        public CharacterDefinition MeleeStats;
        public CharacterDefinition RangedStats;
        public CharacterDefinition BossStats;
        public EnemyArchetype Melee;
        public EnemyArchetype Ranged;
        public EnemyArchetype Boss;
        public WeaponDefinition Sword;
        public AbilityDefinition Fireball;
        public AbilityDefinition Shockwave;
        public AbilityDefinition Bolt;
        public AbilityDefinition EnemyShot;
        public AbilityDefinition BossSlam;
        public LevelCurve Curve;
        public readonly List<ScriptableObject> Assets = new List<ScriptableObject>();

        AttackDefinition[] playerLight;
        AttackDefinition[] playerHeavy;
        AttackDefinition playerAir;
        AttackDefinition[] swordLight;
        AttackDefinition[] swordHeavy;
        AttackDefinition swordAir;
        AttackDefinition[] meleeLight;
        AttackDefinition[] meleeHeavy;
        AttackDefinition[] bossLight;
        AttackDefinition[] bossHeavy;
        AttackDefinition bossAir;

        public static GameContent Create()
        {
            var content = new GameContent();
            content.Build();
            content.Link();
            return content;
        }

        public void Link()
        {
            Chain(playerLight);
            Chain(playerHeavy);
            Chain(swordLight);
            Chain(swordHeavy);
            Chain(meleeLight);
            Chain(meleeHeavy);
            Chain(bossLight);
            Chain(bossHeavy);
            if (playerAir != null) playerAir.Next = null;
            if (swordAir != null) swordAir.Next = null;
            if (bossAir != null) bossAir.Next = null;

            AssignFighter(Player, playerLight, playerHeavy, playerAir);
            AssignFighter(MeleeStats, meleeLight, meleeHeavy, null);
            AssignFighter(RangedStats, null, null, null);
            AssignFighter(BossStats, bossLight, bossHeavy, bossAir);

            if (Sword != null)
            {
                Sword.LightChain = swordLight;
                Sword.HeavyChain = swordHeavy;
                Sword.Aerial = swordAir;
            }

            BindEnemy(Melee, MeleeStats, meleeLight, meleeHeavy, null, null, new[]
            {
                PatternStep.Light, PatternStep.Light, PatternStep.Heavy
            });
            BindEnemy(Ranged, RangedStats, null, null, null, EnemyShot, new[] { PatternStep.Ability });
            BindEnemy(Boss, BossStats, bossLight, bossHeavy, bossAir, BossSlam, new[]
            {
                PatternStep.Light, PatternStep.Light, PatternStep.Heavy,
                PatternStep.Light, PatternStep.Heavy, PatternStep.Ability
            });
            if (Melee != null) Melee.WeaponDropId = SwordId;
        }

        void Build()
        {
            playerLight = new[]
            {
                Attack("player-light-1", AttackKind.Light, 0.08f, 0.09f, 0.12f, 1f, 3.2f, 0.16f, 0f, false, 0.36f, 0.22f),
                Attack("player-light-2", AttackKind.Light, 0.07f, 0.09f, 0.12f, 1.15f, 3.6f, 0.18f, 0f, false, 0.36f, 0.22f),
                Attack("player-light-3", AttackKind.Light, 0.1f, 0.12f, 0.2f, 1.7f, 5.6f, 0.32f, 4.4f, false, 0.16f, 0.34f)
            };
            playerHeavy = new[]
            {
                Attack("player-heavy-1", AttackKind.Heavy, 0.16f, 0.12f, 0.18f, 1.85f, 4.6f, 0.28f, 0f, false, 0.4f, 0.3f),
                Attack("player-heavy-2", AttackKind.Heavy, 0.18f, 0.14f, 0.26f, 2.55f, 7f, 0.4f, 6.4f, false, 0.12f, 0.42f)
            };
            playerAir = Attack("player-aerial", AttackKind.Light, 0.06f, 0.12f, 0.16f, 1.3f, 2.6f, 0.2f, 2.4f, true, 0f, 0.24f);

            swordLight = new[]
            {
                Attack("sword-light-1", AttackKind.Light, 0.08f, 0.1f, 0.12f, 1.3f, 3.8f, 0.18f, 0f, false, 0.36f, 0.22f),
                Attack("sword-light-2", AttackKind.Light, 0.07f, 0.1f, 0.12f, 1.5f, 4.2f, 0.2f, 0f, false, 0.36f, 0.22f),
                Attack("sword-light-3", AttackKind.Light, 0.1f, 0.13f, 0.2f, 2.15f, 6.4f, 0.34f, 5f, false, 0.16f, 0.34f)
            };
            swordHeavy = new[]
            {
                Attack("sword-heavy-1", AttackKind.Heavy, 0.16f, 0.12f, 0.18f, 2.2f, 5.2f, 0.3f, 1.5f, false, 0.4f, 0.3f),
                Attack("sword-heavy-2", AttackKind.Heavy, 0.18f, 0.15f, 0.26f, 3.1f, 8f, 0.42f, 7.2f, false, 0.12f, 0.42f)
            };
            swordAir = Attack("sword-aerial", AttackKind.Light, 0.06f, 0.12f, 0.16f, 1.65f, 3f, 0.22f, 3f, true, 0f, 0.24f);

            meleeLight = new[]
            {
                Attack("melee-light-1", AttackKind.Light, 0.12f, 0.1f, 0.16f, 1f, 2.8f, 0.2f, 0f, false, 0.32f, 0.28f),
                Attack("melee-light-2", AttackKind.Light, 0.1f, 0.1f, 0.18f, 1.2f, 3.4f, 0.24f, 1.2f, false, 0.28f, 0.3f)
            };
            meleeHeavy = new[]
            {
                Attack("melee-heavy-1", AttackKind.Heavy, 0.22f, 0.14f, 0.28f, 1.7f, 5f, 0.36f, 3.5f, false, 0.1f, 0.45f)
            };

            bossLight = new[]
            {
                Attack("boss-light-1", AttackKind.Light, 0.14f, 0.12f, 0.16f, 1.15f, 3.4f, 0.22f, 0f, false, 0.38f, 0.28f),
                Attack("boss-light-2", AttackKind.Light, 0.12f, 0.12f, 0.16f, 1.25f, 3.8f, 0.24f, 0f, false, 0.38f, 0.28f),
                Attack("boss-light-3", AttackKind.Light, 0.14f, 0.14f, 0.22f, 1.6f, 5.5f, 0.34f, 3.2f, false, 0.2f, 0.36f)
            };
            bossHeavy = new[]
            {
                Attack("boss-heavy-1", AttackKind.Heavy, 0.24f, 0.14f, 0.24f, 1.9f, 6f, 0.36f, 2f, false, 0.34f, 0.4f),
                Attack("boss-heavy-2", AttackKind.Heavy, 0.26f, 0.16f, 0.3f, 2.4f, 7.5f, 0.45f, 6f, false, 0.12f, 0.5f)
            };
            bossAir = Attack("boss-aerial", AttackKind.Light, 0.1f, 0.12f, 0.18f, 1.4f, 3f, 0.24f, 2.5f, true, 0f, 0.3f);

            Player = Character("player", "Knight", 6, 4, 5, 6, 80, 20, 5.4f);
            MeleeStats = Character("melee-grunt", "Grunt", 4, 2, 1, 3, 40, 10, 4.3f);
            RangedStats = Character("ranged-caster", "Caster", 3, 1, 6, 4, 32, 28, 4.1f);
            BossStats = Character("boss-king", "King", 8, 6, 4, 3, 150, 40, 3.5f);

            Sword = ScriptableObject.CreateInstance<WeaponDefinition>();
            Sword.Id = SwordId;
            Sword.DisplayName = "Broadsword";
            Sword.StrengthMod = 4;
            Sword.DefenseMod = 0;
            Sword.MagicMod = 0;
            Sword.AgilityMod = 1;
            Sword.name = SwordId;
            Assets.Add(Sword);

            Fireball = Ability(FireballId, "Fireball", Element.Fire, 8f, 0.45f, DeliveryKind.Projectile, 16f, 0.42f, 10f, 3.5f, 0.2f, 0f);
            Shockwave = Ability(ShockwaveId, "Shockwave", Element.Nature, 14f, 1.15f, DeliveryKind.Area, 18f, 2.15f, 0f, 6.5f, 0.36f, 5.2f);
            Bolt = Ability(BoltId, "Bolt", Element.Lightning, 12f, 0.7f, DeliveryKind.Projectile, 28f, 0.36f, 13f, 4.2f, 0.26f, 2.8f);
            EnemyShot = Ability("enemy-shot", "Enemy Shot", Element.Ice, 6f, 1.15f, DeliveryKind.Projectile, 11f, 0.4f, 8f, 2.6f, 0.22f, 0f);
            BossSlam = Ability("boss-slam", "Slam", Element.Fire, 10f, 2.4f, DeliveryKind.Area, 16f, 1.85f, 0f, 5.5f, 0.4f, 6f);

            Melee = Enemy("melee-grunt-archetype", "Grunt", EnemyRole.Melee, 1.05f, 0.22f, 40, 15, 10f);
            Ranged = Enemy("ranged-caster-archetype", "Caster", EnemyRole.Ranged, 4.35f, 0.3f, 45, 18, 11f);
            Boss = Enemy("boss-king-archetype", "King", EnemyRole.Boss, 1.3f, 0.34f, 100, 55, 12f);

            Curve = ScriptableObject.CreateInstance<LevelCurve>();
            Curve.BaseXp = 40;
            Curve.XpPerLevel = 30;
            Curve.name = "level-curve";
            Assets.Add(Curve);
        }

        static void Chain(AttackDefinition[] chain)
        {
            if (chain == null) return;
            for (int i = 0; i < chain.Length; i++)
            {
                if (chain[i] == null) continue;
                chain[i].ChainIndex = i;
                chain[i].Next = i + 1 < chain.Length ? chain[i + 1] : null;
            }
        }

        static void AssignFighter(CharacterDefinition definition, AttackDefinition[] light, AttackDefinition[] heavy, AttackDefinition aerial)
        {
            if (definition == null) return;
            definition.LightChain = light;
            definition.HeavyChain = heavy;
            definition.Aerial = aerial;
        }

        static void BindEnemy(
            EnemyArchetype archetype,
            CharacterDefinition stats,
            AttackDefinition[] light,
            AttackDefinition[] heavy,
            AttackDefinition aerial,
            AbilityDefinition ability,
            PatternStep[] pattern)
        {
            if (archetype == null) return;
            archetype.Stats = stats;
            archetype.LightChain = light;
            archetype.HeavyChain = heavy;
            archetype.Aerial = aerial;
            archetype.RangedAbility = ability;
            archetype.Pattern = pattern;
        }

        AttackDefinition Attack(
            string id,
            AttackKind kind,
            float startup,
            float active,
            float recovery,
            float multiplier,
            float knockback,
            float hitstun,
            float launch,
            bool aerial,
            float comboWindow,
            float cooldown)
        {
            var attack = ScriptableObject.CreateInstance<AttackDefinition>();
            attack.Id = id;
            attack.name = id;
            attack.Kind = kind;
            attack.Startup = startup;
            attack.Active = active;
            attack.Recovery = recovery;
            attack.DamageMultiplier = multiplier;
            attack.Knockback = knockback;
            attack.KnockbackDepth = 0.4f;
            attack.Hitstun = hitstun;
            attack.Launch = launch;
            attack.Aerial = aerial;
            attack.ComboWindow = comboWindow;
            attack.Cooldown = cooldown;
            attack.HitboxOffset = aerial
                ? new Vector3(0.72f, 0.48f, 0f)
                : kind == AttackKind.Heavy ? new Vector3(0.95f, 0.78f, 0f) : new Vector3(0.8f, 0.72f, 0f);
            attack.HitboxSize = aerial
                ? new Vector2(1.05f, 0.75f)
                : kind == AttackKind.Heavy ? new Vector2(1.3f, 1.05f) : new Vector2(1.15f, 0.95f);
            attack.HitboxDepth = 1.05f;
            Assets.Add(attack);
            return attack;
        }

        CharacterDefinition Character(string id, string displayName, int strength, int defense, int magic, int agility, int baseHealth, int baseMana, float speed)
        {
            var definition = ScriptableObject.CreateInstance<CharacterDefinition>();
            definition.Id = id;
            definition.name = id;
            definition.DisplayName = displayName;
            definition.Strength = strength;
            definition.Defense = defense;
            definition.Magic = magic;
            definition.Agility = agility;
            definition.BaseHealth = baseHealth;
            definition.BaseMana = baseMana;
            definition.BaseMoveSpeed = speed;
            Assets.Add(definition);
            return definition;
        }

        AbilityDefinition Ability(
            string id,
            string displayName,
            Element element,
            float mana,
            float cooldown,
            DeliveryKind delivery,
            float damage,
            float radius,
            float speed,
            float knockback,
            float hitstun,
            float launch)
        {
            var ability = ScriptableObject.CreateInstance<AbilityDefinition>();
            ability.Id = id;
            ability.name = id;
            ability.DisplayName = displayName;
            ability.Element = element;
            ability.ManaCost = mana;
            ability.Cooldown = cooldown;
            ability.Delivery = delivery;
            ability.Damage = damage;
            ability.Radius = radius;
            ability.Speed = speed;
            ability.Knockback = knockback;
            ability.Hitstun = hitstun;
            ability.Launch = launch;
            Assets.Add(ability);
            return ability;
        }

        EnemyArchetype Enemy(string id, string displayName, EnemyRole role, float range, float retreat, int xp, int gold, float aggro)
        {
            var archetype = ScriptableObject.CreateInstance<EnemyArchetype>();
            archetype.Id = id;
            archetype.name = id;
            archetype.DisplayName = displayName;
            archetype.Role = role;
            archetype.PreferredRange = range;
            archetype.RetreatHealthFraction = retreat;
            archetype.Xp = xp;
            archetype.Gold = gold;
            archetype.AggroRange = aggro;
            Assets.Add(archetype);
            return archetype;
        }
    }
}
