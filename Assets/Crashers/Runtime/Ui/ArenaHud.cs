using Crashers.Characters;
using Crashers.Magic;
using Crashers.Progression;
using Crashers.Weapons;
using UnityEngine;
using UnityEngine.UI;

namespace Crashers.Ui
{
    public sealed class ArenaHud : MonoBehaviour
    {
        Health health;
        Experience experience;
        LevelCurve curve;
        Wallet wallet;
        Mana mana;
        Unlocks unlocks;
        Equipment equipment;
        Motor2D5 motor;
        Text label;
        string boltId;
        int boltLevel;

        public static ArenaHud Build(
            Health life,
            Experience xp,
            LevelCurve levelCurve,
            Wallet gold,
            Mana pool,
            Unlocks known,
            Equipment gear,
            Motor2D5 playerMotor,
            string lockedAbilityId,
            int unlockLevel)
        {
            var canvasObject = new GameObject("HUD");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var panel = new GameObject("Panel");
            panel.transform.SetParent(canvasObject.transform, false);
            var panelImage = panel.AddComponent<Image>();
            panelImage.sprite = White();
            panelImage.color = new Color(0.04f, 0.05f, 0.08f, 0.72f);
            panelImage.raycastTarget = false;
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 1f);
            panelRect.anchorMax = new Vector2(0f, 1f);
            panelRect.pivot = new Vector2(0f, 1f);
            panelRect.anchoredPosition = new Vector2(24f, -24f);
            panelRect.sizeDelta = new Vector2(620f, 380f);

            var textObject = new GameObject("Readout");
            textObject.transform.SetParent(panel.transform, false);
            var text = textObject.AddComponent<Text>();
            text.font = LoadFont();
            text.fontSize = 24;
            text.color = Color.white;
            text.alignment = TextAnchor.UpperLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            var textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(16f, 12f);
            textRect.offsetMax = new Vector2(-16f, -12f);

            var hud = canvasObject.AddComponent<ArenaHud>();
            hud.health = life;
            hud.experience = xp;
            hud.curve = levelCurve;
            hud.wallet = gold;
            hud.mana = pool;
            hud.unlocks = known;
            hud.equipment = gear;
            hud.motor = playerMotor;
            hud.label = text;
            hud.boltId = lockedAbilityId;
            hud.boltLevel = unlockLevel;
            hud.Refresh();
            return hud;
        }

        void Update() => Refresh();

        void Refresh()
        {
            if (label == null) return;
            float hp = health != null ? health.Current : 0f;
            float hpMax = health != null ? health.Max : 1f;
            int level = experience != null ? experience.Level : 1;
            int xp = experience != null ? experience.Xp : 0;
            int need = curve != null ? curve.RequiredForNext(level) : 0;
            int gold = wallet != null ? wallet.Gold : 0;
            float mp = mana != null ? mana.Current : 0f;
            float mpMax = mana != null ? mana.Max : 1f;
            string weapon = equipment != null && equipment.Equipped != null ? equipment.Equipped.DisplayName : "Unarmed";
            bool boltReady = unlocks != null && unlocks.IsUnlocked(boltId);
            string bolt = boltReady ? "Bolt ready (O / RB)" : "Bolt locked until level " + boltLevel;
            string pickup = "";
            if (motor != null && WeaponPickup.FindClosest(motor.GroundX, motor.GroundZ, 0.95f, 0.75f) != null)
                pickup = "\nWeapon nearby — E / East to take it";
            string alive = health != null && !health.IsAlive ? "\nDowned — respawning" : "";
            label.text =
                "HP " + Mathf.CeilToInt(hp) + " / " + Mathf.CeilToInt(hpMax) +
                "\nLevel " + level +
                "\nXP " + xp + " / " + need +
                "\nGold " + gold +
                "\nMP " + Mathf.CeilToInt(mp) + " / " + Mathf.CeilToInt(mpMax) +
                "\n" + weapon +
                "\n" + bolt +
                "\nWASD move   Space jump   J light   K heavy   U / I magic" +
                pickup +
                alive;
        }

        static Font LoadFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return font;
        }

        static Sprite White()
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        }
    }
}
