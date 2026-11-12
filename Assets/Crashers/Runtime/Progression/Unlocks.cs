using System.Collections.Generic;
using UnityEngine;

namespace Crashers.Progression
{
    public sealed class Unlocks : MonoBehaviour
    {
        readonly List<string> ids = new List<string>();

        public IReadOnlyList<string> Ids => ids;

        public bool IsUnlocked(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            for (int i = 0; i < ids.Count; i++)
            {
                if (ids[i] == id) return true;
            }
            return false;
        }

        public bool Grant(string id)
        {
            if (string.IsNullOrEmpty(id) || IsUnlocked(id)) return false;
            ids.Add(id);
            return true;
        }

        public void CopyTo(List<string> target)
        {
            target.Clear();
            for (int i = 0; i < ids.Count; i++) target.Add(ids[i]);
        }
    }
}
