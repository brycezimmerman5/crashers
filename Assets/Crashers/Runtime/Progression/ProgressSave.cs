using System;
using System.Collections.Generic;

namespace Crashers.Progression
{
    [Serializable]
    public sealed class ProgressSave
    {
        public int Level = 1;
        public int Xp;
        public int Gold;
        public List<string> Unlocked = new List<string>();
    }
}
