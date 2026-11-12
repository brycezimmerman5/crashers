using Crashers.Characters;
using Crashers.Content;
using UnityEngine;

namespace Crashers.Combat
{
    /// <summary>
    /// Short red burst at a hit. Fades out and destroys itself in under a second.
    /// </summary>
    public sealed class BloodBurst : MonoBehaviour
    {
        const int Drops = 6;
        const float Life = 0.48f;

        static Sprite pixel;

        SpriteRenderer[] drops;
        Vector3[] drift;
        Color[] colors;
        float age;

        public static void Spawn(Hurtbox box, Entity attacker)
        {
            if (box == null) return;
            float x = box.WorldX;
            float z = box.WorldZ;
            if (attacker != null)
            {
                float dx = x - attacker.transform.position.x;
                if (Mathf.Abs(dx) > 0.04f) x -= Mathf.Sign(dx) * 0.14f;
            }
            float height = Mathf.Lerp(box.WorldHeightMin, box.WorldHeightMax, 0.58f);
            var host = new GameObject("Blood");
            Motor2D5.Place(host.transform, x, z, height);
            host.AddComponent<BloodBurst>();
        }

        void Awake()
        {
            if (pixel == null) pixel = PlaceholderSprites.WhiteQuad();
            drops = new SpriteRenderer[Drops];
            drift = new Vector3[Drops];
            colors = new Color[Drops];
            for (int i = 0; i < Drops; i++)
            {
                var drop = new GameObject("Drop");
                drop.transform.SetParent(transform, false);
                float angle = Random.Range(0f, 360f);
                float reach = Random.Range(0.02f, 0.2f);
                drop.transform.localPosition = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad) * reach, Mathf.Sin(angle * Mathf.Deg2Rad) * reach * 0.7f, 0f);
                drop.transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 180f));
                float length = Random.Range(0.16f, 0.42f);
                float width = Random.Range(0.04f, 0.1f);
                drop.transform.localScale = new Vector3(length, width, 1f);
                var renderer = drop.AddComponent<SpriteRenderer>();
                renderer.sprite = pixel;
                var color = Color.Lerp(new Color(0.55f, 0.02f, 0.03f, 1f), new Color(0.9f, 0.08f, 0.06f, 1f), Random.value);
                renderer.color = color;
                var sorter = drop.AddComponent<DepthSorter>();
                sorter.Bias = 8;
                drops[i] = renderer;
                colors[i] = color;
                drift[i] = drop.transform.localPosition.normalized * Random.Range(0.15f, 0.45f);
            }
        }

        void Update()
        {
            age += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(age / Life);
            for (int i = 0; i < drops.Length; i++)
            {
                if (drops[i] == null) continue;
                drops[i].transform.localPosition += drift[i] * Time.unscaledDeltaTime;
                Color color = colors[i];
                color.a = 1f - t;
                drops[i].color = color;
            }
            if (t < 1f) return;
            Destroy(gameObject);
        }
    }
}
