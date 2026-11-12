using UnityEngine;

namespace Crashers.Characters
{
    public enum Faction
    {
        Player = 0,
        Enemy = 1
    }

    public sealed class Entity : MonoBehaviour
    {
        Health health;

        public Faction Faction { get; private set; }
        public int Facing { get; private set; } = 1;
        public bool IsAlive => health != null && health.IsAlive;

        public void Configure(Faction faction, Health body)
        {
            Faction = faction;
            health = body;
            Facing = 1;
        }

        public void SetFacing(int sign)
        {
            if (sign == 0) return;
            Facing = sign > 0 ? 1 : -1;
        }

        public void FaceTowards(float worldX, float fromX)
        {
            float delta = worldX - fromX;
            if (Mathf.Abs(delta) > 0.05f) SetFacing(delta > 0f ? 1 : -1);
        }
    }
}
