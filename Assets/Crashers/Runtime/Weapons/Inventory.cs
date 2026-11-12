using System.Collections.Generic;
using UnityEngine;

namespace Crashers.Weapons
{
    public sealed class Inventory : MonoBehaviour
    {
        readonly List<WeaponDefinition> owned = new List<WeaponDefinition>();

        public IReadOnlyList<WeaponDefinition> Owned => owned;

        public bool Contains(string id)
        {
            for (int i = 0; i < owned.Count; i++)
            {
                if (owned[i] != null && owned[i].Id == id) return true;
            }
            return false;
        }

        public void Add(WeaponDefinition weapon)
        {
            if (weapon == null || Contains(weapon.Id)) return;
            owned.Add(weapon);
        }
    }
}
