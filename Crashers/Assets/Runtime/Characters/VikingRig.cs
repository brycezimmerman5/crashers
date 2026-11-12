using Crashers.Combat;
using Crashers.Content;
using UnityEngine;

namespace Crashers.Characters
{
    /// <summary>
    /// Builds the player visual from the viking PSD.
    /// Prefers the imported rig. Otherwise parents each part to the bone the clip rotates.
    /// </summary>
    public static class VikingRig
    {
        struct BonePart
        {
            public string Bone;
            public float BoneX;
            public float BoneY;
            public float RotZ;
            public float RotW;
            public float SpriteX;
            public float SpriteY;
            public float SpriteW;
            public float SpriteH;
            public int SortBias;
        }

        public static bool TryMount(
            Transform visual,
            Entity entity,
            CombatController combat,
            Motor2D5 motor,
            Hurtbox hurtbox,
            SpriteFlash flash,
            out SpriteRenderer sortBody,
            out float bodyWidth)
        {
            sortBody = null;
            bodyWidth = 0.9f;
            if (visual == null) return false;

            var facing = new GameObject("Viking");
            facing.transform.SetParent(visual, false);
            var rigObject = new GameObject("Rig");
            rigObject.transform.SetParent(facing.transform, false);

            bool built = TryInstantiatePrefab(rigObject.transform) || TryBuildParts(rigObject.transform);
            if (!built)
            {
                Object.Destroy(facing);
                return false;
            }

            Transform animated = AnimatedRoot(rigObject.transform);
            Animator animator = animated.GetComponent<Animator>();
            if (animator == null) animator = animated.gameObject.AddComponent<Animator>();
            animator.runtimeAnimatorController = VikingArt.Controller;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.updateMode = AnimatorUpdateMode.Normal;

            SpriteRenderer[] renderers = rigObject.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer renderer = renderers[i];
                if (renderer.GetComponent<DepthSorter>() != null) continue;
                DepthSorter sorter = renderer.gameObject.AddComponent<DepthSorter>();
                sorter.Bias = BiasFor(renderer.name);
            }

            Fit(rigObject.transform, renderers, hurtbox, motor, entity, out sortBody, out bodyWidth);
            if (flash != null && renderers.Length > 0) flash.Adopt(renderers);

            if (renderers.Length == 0)
            {
                Object.Destroy(facing);
                return false;
            }

            VikingPlayer driver = facing.AddComponent<VikingPlayer>();
            driver.Bind(entity, VikingArt.HasController ? animator : null, combat);
            return true;
        }

        static bool TryInstantiatePrefab(Transform rig)
        {
            if (VikingArt.Prefab == null) return false;
            GameObject instance = Object.Instantiate(VikingArt.Prefab, rig);
            instance.name = "vikingcrashers";
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            SpriteRenderer[] renderers = instance.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i].sprite != null) return true;
            }
            Object.Destroy(instance);
            return false;
        }

        static bool TryBuildParts(Transform rig)
        {
            if (!VikingArt.HasSprites) return false;
            AddPart(rig, VikingArt.LeftHand, Part("bone_1", 522.1392f, 429.3266f, -0.862999f, 0.5052056f, 400f, 268f, 165f, 165f, 2));
            AddPart(rig, VikingArt.RightHand, Part("bone_2", 995.5594f, 399.2474f, -0.55437595f, 0.83226645f, 951f, 264f, 135f, 144f, 3));
            AddPart(rig, VikingArt.RightFoot, Part("bone_3", 853.0102f, 171.69186f, -0.53558475f, 0.84448147f, 779f, 44f, 221f, 139f, 0));
            AddPart(rig, VikingArt.LeftFoot, Part("bone_4", 634.6092f, 172.99965f, -0.751445f, 0.65979576f, 517f, 40f, 222f, 142f, 0));
            AddPart(rig, VikingArt.Torso, Part("bone_5", 741.848f, 456.7902f, -0.6941399f, 0.7198401f, 569f, 209f, 302f, 288f, 1));
            AddPart(rig, VikingArt.Head, Part("bone_6", 714.3844f, 501.25507f, 0.67616737f, 0.73674804f, 502f, 370f, 490f, 642f, 4));
            return true;
        }

        static BonePart Part(
            string bone, float boneX, float boneY, float rotZ, float rotW,
            float spriteX, float spriteY, float spriteW, float spriteH, int sortBias)
        {
            return new BonePart
            {
                Bone = bone,
                BoneX = boneX,
                BoneY = boneY,
                RotZ = rotZ,
                RotW = rotW,
                SpriteX = spriteX,
                SpriteY = spriteY,
                SpriteW = spriteW,
                SpriteH = spriteH,
                SortBias = sortBias
            };
        }

        static void AddPart(Transform rig, Sprite sprite, BonePart part)
        {
            var boneObject = new GameObject(part.Bone);
            boneObject.transform.SetParent(rig, false);
            Quaternion rotation = new Quaternion(0f, 0f, part.RotZ, part.RotW);
            Vector2 pivot = new Vector2(VikingArt.DocumentWidth * 0.5f, 0f);
            Vector2 bonePixel = new Vector2(part.BoneX, part.BoneY);
            Vector2 center = new Vector2(part.SpriteX + part.SpriteW * 0.5f, part.SpriteY + part.SpriteH * 0.5f);
            Vector2 boneLocal = (bonePixel - pivot) / VikingArt.PixelsPerUnit;
            Vector3 delta = new Vector3((center.x - bonePixel.x) / VikingArt.PixelsPerUnit, (center.y - bonePixel.y) / VikingArt.PixelsPerUnit, 0f);
            boneObject.transform.localPosition = new Vector3(boneLocal.x, boneLocal.y, 0f);
            boneObject.transform.localRotation = rotation;
            boneObject.transform.localScale = Vector3.one;

            var partObject = new GameObject(sprite != null ? sprite.name : part.Bone);
            partObject.transform.SetParent(boneObject.transform, false);
            partObject.transform.localPosition = Quaternion.Inverse(rotation) * delta;
            partObject.transform.localRotation = Quaternion.Inverse(rotation);
            partObject.transform.localScale = Vector3.one;
            SpriteRenderer renderer = partObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            DepthSorter sorter = partObject.AddComponent<DepthSorter>();
            sorter.Bias = part.SortBias;
        }

        static void Fit(
            Transform rig,
            SpriteRenderer[] renderers,
            Hurtbox hurtbox,
            Motor2D5 motor,
            Entity entity,
            out SpriteRenderer sortBody,
            out float bodyWidth)
        {
            sortBody = null;
            bodyWidth = 0.9f;
            int bestBias = int.MinValue;
            Bounds bounds = new Bounds(rig.position, Vector3.zero);
            bool any = false;
            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer renderer = renderers[i];
                if (renderer == null || renderer.sprite == null) continue;
                EncapsulateSprite(ref bounds, ref any, renderer);
                int bias = BiasFor(renderer.name);
                if (bias >= bestBias)
                {
                    bestBias = bias;
                    sortBody = renderer;
                }
            }
            if (!any) return;

            float height = Mathf.Max(0.01f, bounds.size.y);
            float scale = VikingArt.TargetHeight / height;
            float bottom = bounds.min.y - rig.position.y;
            float centerX = bounds.center.x - rig.position.x;
            rig.localScale = new Vector3(scale, scale, 1f);
            rig.localPosition = new Vector3(-centerX * scale, -bottom * scale, 0f);
            bodyWidth = bounds.size.x * scale * 0.72f;
            if (hurtbox != null && entity != null)
            {
                float halfWidth = bounds.size.x * scale * 0.32f;
                float halfDepth = hurtbox.HalfDepth;
                hurtbox.Configure(entity, motor, halfWidth, halfDepth, 0.02f, VikingArt.TargetHeight * 0.92f);
            }
        }

        static void EncapsulateSprite(ref Bounds bounds, ref bool any, SpriteRenderer renderer)
        {
            Bounds local = renderer.sprite.bounds;
            Vector3 center = local.center;
            Vector3 extents = local.extents;
            Vector3[] corners =
            {
                new Vector3(-extents.x, -extents.y, 0f),
                new Vector3(-extents.x, extents.y, 0f),
                new Vector3(extents.x, -extents.y, 0f),
                new Vector3(extents.x, extents.y, 0f)
            };
            for (int i = 0; i < corners.Length; i++)
            {
                Vector3 world = renderer.transform.TransformPoint(center + corners[i]);
                if (!any)
                {
                    bounds = new Bounds(world, Vector3.zero);
                    any = true;
                }
                else bounds.Encapsulate(world);
            }
        }

        static Transform AnimatedRoot(Transform rig)
        {
            Transform[] transforms = rig.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                if (transforms[i].name == "bone_6" && transforms[i].parent != null) return transforms[i].parent;
            }
            return rig;
        }

        static int BiasFor(string partName)
        {
            if (string.IsNullOrEmpty(partName)) return 1;
            string name = partName.ToLowerInvariant();
            if (name.Contains("head")) return 4;
            if (name.Contains("hand")) return 3;
            if (name.Contains("torso") || name.Contains("body")) return 1;
            if (name.Contains("foot") || name.Contains("leg")) return 0;
            return 1;
        }
    }
}
