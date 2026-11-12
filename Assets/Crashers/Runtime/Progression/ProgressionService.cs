using System;
using System.Collections.Generic;
using System.IO;
using Crashers.Characters;
using Crashers.Enemies;
using UnityEngine;

namespace Crashers.Progression
{
    public sealed class ProgressionService : MonoBehaviour
    {
        Experience experience;
        Wallet wallet;
        Unlocks unlocks;
        CharacterStats stats;
        LevelCurve curve;
        string fireballId;
        string shockwaveId;
        string boltId;
        int boltUnlockLevel = 2;

        public Experience Experience => experience;
        public Wallet Wallet => wallet;
        public Unlocks Unlocks => unlocks;

        public void Configure(
            Experience xp,
            Wallet gold,
            Unlocks known,
            CharacterStats body,
            LevelCurve levelCurve,
            string starterProjectileId,
            string starterAreaId,
            string lockedAbilityId,
            int unlockLevel)
        {
            experience = xp;
            wallet = gold;
            unlocks = known;
            stats = body;
            curve = levelCurve;
            fireballId = starterProjectileId;
            shockwaveId = starterAreaId;
            boltId = lockedAbilityId;
            boltUnlockLevel = unlockLevel < 1 ? 1 : unlockLevel;
        }

        public void LoadOrNew()
        {
            EnsureStarters();
            string path = SavePath();
            if (File.Exists(path))
            {
                try
                {
                    ProgressSave save = JsonUtility.FromJson<ProgressSave>(File.ReadAllText(path));
                    if (save != null)
                    {
                        if (experience != null) experience.Set(save.Level, save.Xp);
                        if (wallet != null) wallet.Set(save.Gold);
                        if (save.Unlocked != null && unlocks != null)
                        {
                            for (int i = 0; i < save.Unlocked.Count; i++) unlocks.Grant(save.Unlocked[i]);
                        }
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogWarning("Crashers progress could not be read and was started fresh. " + exception.Message);
                    if (experience != null) experience.Set(1, 0);
                    if (wallet != null) wallet.Set(0);
                }
            }
            else
            {
                if (experience != null) experience.Set(1, 0);
                if (wallet != null) wallet.Set(0);
            }

            EnsureStarters();
            int level = experience != null ? experience.Level : 1;
            if (level >= boltUnlockLevel && unlocks != null) unlocks.Grant(boltId);
            if (stats != null) stats.SetLevelBonus(Mathf.Max(0, level - 1));
        }

        public void RegisterKill(EnemyArchetype archetype)
        {
            if (archetype == null) return;
            if (wallet != null) wallet.Add(archetype.Gold);
            if (experience != null) experience.AddXp(archetype.Xp);
            if (experience != null && curve != null)
            {
                int guard = 0;
                while (guard < 24 && experience.Xp >= curve.RequiredForNext(experience.Level))
                {
                    experience.SpendXp(curve.RequiredForNext(experience.Level));
                    experience.SetLevel(experience.Level + 1);
                    guard++;
                }
            }

            int level = experience != null ? experience.Level : 1;
            if (stats != null) stats.SetLevelBonus(Mathf.Max(0, level - 1));
            if (level >= boltUnlockLevel && unlocks != null) unlocks.Grant(boltId);
            Save();
        }

        void EnsureStarters()
        {
            if (unlocks == null) return;
            unlocks.Grant(fireballId);
            unlocks.Grant(shockwaveId);
        }

        void Save()
        {
            try
            {
                var save = new ProgressSave();
                if (experience != null)
                {
                    save.Level = experience.Level;
                    save.Xp = experience.Xp;
                }
                if (wallet != null) save.Gold = wallet.Gold;
                if (unlocks != null)
                {
                    if (save.Unlocked == null) save.Unlocked = new List<string>();
                    unlocks.CopyTo(save.Unlocked);
                }
                File.WriteAllText(SavePath(), JsonUtility.ToJson(save, true));
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Crashers progress could not be saved. " + exception.Message);
            }
        }

        static string SavePath()
        {
            return Path.Combine(Application.persistentDataPath, "crashers-progress.json");
        }
    }
}
