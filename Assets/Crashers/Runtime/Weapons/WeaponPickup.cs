using System.Collections.Generic;
using Crashers.Characters;
using UnityEngine;

namespace Crashers.Weapons
{
    public sealed class WeaponPickup : MonoBehaviour
    {
        static readonly List<WeaponPickup> active = new List<WeaponPickup>();

        WeaponDefinition weapon;
        float groundX;
        float groundZ;
        float bob;
        bool available = true;

        public static IReadOnlyList<WeaponPickup> Active => active;
        public WeaponDefinition Weapon => weapon;
        public bool IsAvailable => available;
        public float GroundX => groundX;
        public float GroundZ => groundZ;

        void OnEnable()
        {
            if (!active.Contains(this)) active.Add(this);
        }

        void OnDisable() => active.Remove(this);

        public void Spawn(WeaponDefinition definition, float x, float z, Sprite sprite, Color color)
        {
            weapon = definition;
            groundX = x;
            groundZ = z;
            available = true;
            var renderer = gameObject.GetComponent<SpriteRenderer>();
            if (renderer == null) renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            if (gameObject.GetComponent<DepthSorter>() == null) gameObject.AddComponent<DepthSorter>();
            Motor2D5.Place(transform, groundX, groundZ, 0.15f);
        }

        void Update()
        {
            if (!available) return;
            bob += Time.deltaTime * 3f;
            float height = 0.2f + Mathf.Sin(bob) * 0.08f;
            Motor2D5.Place(transform, groundX, groundZ, height);
        }

        public bool Give(Inventory inventory, Equipment equipment)
        {
            if (!available || weapon == null || inventory == null || equipment == null) return false;
            available = false;
            if (equipment.Equipped == null) equipment.Equip(weapon);
            else inventory.Add(weapon);
            Destroy(gameObject);
            return true;
        }

        public static WeaponPickup FindClosest(float x, float z, float maxDx, float maxDz)
        {
            WeaponPickup best = null;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < active.Count; i++)
            {
                WeaponPickup pickup = active[i];
                if (pickup == null || !pickup.available) continue;
                float dx = Mathf.Abs(pickup.groundX - x);
                float dz = Mathf.Abs(pickup.groundZ - z);
                if (dx > maxDx || dz > maxDz) continue;
                float distance = dx * dx + dz * dz;
                if (distance >= bestDistance) continue;
                bestDistance = distance;
                best = pickup;
            }
            return best;
        }
    }
}
