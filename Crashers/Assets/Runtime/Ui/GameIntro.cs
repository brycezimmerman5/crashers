using System.Collections;
using Crashers.Bootstrap;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Crashers.Ui
{
    /// <summary>
    /// Title screen with Play and Settings, then a short loading screen into the arena.
    /// </summary>
    public sealed class GameIntro : MonoBehaviour
    {
        const string VolumeKey = "Crashers.MasterVolume";

        ArenaBootstrap game;
        GameObject title;
        GameObject settings;
        GameObject loading;
        RectTransform barFill;
        Text loadingLabel;
        Font font;
        Camera menuCamera;
        bool loadingStarted;

        public static void Open(ArenaBootstrap arena)
        {
            var host = new GameObject("Intro");
            var intro = host.AddComponent<GameIntro>();
            intro.game = arena;
            intro.BuildUi();
        }

        void BuildUi()
        {
            font = LoadFont();
            float volume = PlayerPrefs.GetFloat(VolumeKey, 1f);
            AudioListener.volume = volume;
            menuCamera = CreateMenuCamera();

            var eventObject = new GameObject("Event System");
            eventObject.transform.SetParent(transform, false);
            eventObject.AddComponent<EventSystem>();
            eventObject.AddComponent<InputSystemUIInputModule>();

            var canvasObject = new GameObject("Intro Canvas");
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            var backdrop = CreatePanel(canvasObject.transform, "Backdrop", Vector2.zero, new Vector2(1920f, 1080f));
            backdrop.GetComponent<Image>().color = new Color(0.06f, 0.07f, 0.1f, 1f);
            Stretch(backdrop.GetComponent<RectTransform>());

            title = CreateGroup(canvasObject.transform, "Title");
            settings = CreateGroup(canvasObject.transform, "Settings");
            loading = CreateGroup(canvasObject.transform, "Loading");

            var heading = CreateText(title.transform, "CRASHERS", 92, TextAnchor.MiddleCenter);
            heading.rectTransform.anchoredPosition = new Vector2(0f, 220f);
            heading.rectTransform.sizeDelta = new Vector2(900f, 120f);
            var subtitle = CreateText(title.transform, "A small arena brawler", 28, TextAnchor.MiddleCenter);
            subtitle.color = new Color(0.75f, 0.78f, 0.84f, 1f);
            subtitle.rectTransform.anchoredPosition = new Vector2(0f, 130f);
            subtitle.rectTransform.sizeDelta = new Vector2(700f, 48f);

            CreateButton(title.transform, "Play", new Vector2(0f, -10f), StartLoad);
            CreateButton(title.transform, "Settings", new Vector2(0f, -100f), ShowSettings);

            var settingsTitle = CreateText(settings.transform, "Settings", 64, TextAnchor.MiddleCenter);
            settingsTitle.rectTransform.anchoredPosition = new Vector2(0f, 220f);
            settingsTitle.rectTransform.sizeDelta = new Vector2(700f, 80f);
            var volumeLabel = CreateText(settings.transform, "Master volume", 28, TextAnchor.MiddleCenter);
            volumeLabel.rectTransform.anchoredPosition = new Vector2(0f, 80f);
            volumeLabel.rectTransform.sizeDelta = new Vector2(500f, 40f);
            CreateVolumeSlider(settings.transform, volume);
            CreateButton(settings.transform, "Back", new Vector2(0f, -140f), ShowTitle);

            var loadingTitle = CreateText(loading.transform, "Loading", 64, TextAnchor.MiddleCenter);
            loadingTitle.rectTransform.anchoredPosition = new Vector2(0f, 80f);
            loadingTitle.rectTransform.sizeDelta = new Vector2(700f, 80f);
            loadingLabel = CreateText(loading.transform, "Loading 0%", 28, TextAnchor.MiddleCenter);
            loadingLabel.rectTransform.anchoredPosition = new Vector2(0f, -10f);
            loadingLabel.rectTransform.sizeDelta = new Vector2(400f, 40f);
            barFill = CreateBar(loading.transform);

            settings.SetActive(false);
            loading.SetActive(false);
        }

        void ShowTitle()
        {
            if (loadingStarted) return;
            title.SetActive(true);
            settings.SetActive(false);
        }

        void ShowSettings()
        {
            if (loadingStarted) return;
            title.SetActive(false);
            settings.SetActive(true);
        }

        void StartLoad()
        {
            if (loadingStarted) return;
            loadingStarted = true;
            title.SetActive(false);
            settings.SetActive(false);
            loading.SetActive(true);
            StartCoroutine(LoadGame());
        }

        IEnumerator LoadGame()
        {
            const float duration = 1.2f;
            float elapsed = 0f;
            bool built = false;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                if (barFill != null) barFill.localScale = new Vector3(Mathf.Max(0.02f, progress), 1f, 1f);
                if (loadingLabel != null) loadingLabel.text = "Loading " + Mathf.RoundToInt(progress * 100f) + "%";
                if (!built && progress >= 0.4f && game != null)
                {
                    game.BeginGame();
                    built = true;
                    ReleaseMenuCamera();
                }
                yield return null;
            }
            if (!built && game != null) game.BeginGame();
            ReleaseMenuCamera();
            Destroy(gameObject);
        }

        Camera CreateMenuCamera()
        {
            var cameraObject = new GameObject("Menu Camera");
            cameraObject.transform.SetParent(transform, false);
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 2.9f, -12f);
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 4.85f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.06f, 0.07f, 0.1f, 1f);
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 50f;
            camera.depth = -1f;
            return camera;
        }

        void ReleaseMenuCamera()
        {
            if (menuCamera == null) return;
            menuCamera.enabled = false;
            menuCamera.tag = "Untagged";
        }

        void CreateVolumeSlider(Transform parent, float volume)
        {
            var sliderObject = new GameObject("Volume");
            sliderObject.transform.SetParent(parent, false);
            var rect = sliderObject.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(460f, 28f);
            rect.anchoredPosition = new Vector2(0f, 20f);
            var background = sliderObject.AddComponent<Image>();
            background.sprite = White();
            background.color = new Color(0.14f, 0.15f, 0.18f, 1f);

            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObject.transform, false);
            var fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0f, 0.2f);
            fillAreaRect.anchorMax = new Vector2(1f, 0.8f);
            fillAreaRect.offsetMin = new Vector2(8f, 0f);
            fillAreaRect.offsetMax = new Vector2(-8f, 0f);

            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            var fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            var fillImage = fill.AddComponent<Image>();
            fillImage.sprite = White();
            fillImage.color = new Color(0.35f, 0.62f, 0.95f, 1f);

            var handle = new GameObject("Handle");
            handle.transform.SetParent(sliderObject.transform, false);
            var handleRect = handle.AddComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(22f, 36f);
            var handleImage = handle.AddComponent<Image>();
            handleImage.sprite = White();
            handleImage.color = Color.white;

            var slider = sliderObject.AddComponent<Slider>();
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImage;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = volume;
            slider.onValueChanged.AddListener(SetVolume);
        }

        static void SetVolume(float value)
        {
            AudioListener.volume = value;
            PlayerPrefs.SetFloat(VolumeKey, value);
        }

        RectTransform CreateBar(Transform parent)
        {
            var track = CreatePanel(parent, "Bar", new Vector2(0f, -80f), new Vector2(520f, 22f));
            track.GetComponent<Image>().color = new Color(0.14f, 0.15f, 0.18f, 1f);
            var fill = CreatePanel(track.transform, "Fill", Vector2.zero, new Vector2(520f, 22f));
            var fillImage = fill.GetComponent<Image>();
            fillImage.color = new Color(0.35f, 0.62f, 0.95f, 1f);
            var rect = fill.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.sizeDelta = new Vector2(520f, 0f);
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = new Vector3(0.02f, 1f, 1f);
            return rect;
        }

        void CreateButton(Transform parent, string label, Vector2 position, UnityEngine.Events.UnityAction onClick)
        {
            var buttonObject = CreatePanel(parent, label, position, new Vector2(340f, 68f));
            var image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.16f, 0.18f, 0.24f, 1f);
            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.highlightedColor = new Color(0.28f, 0.34f, 0.46f, 1f);
            colors.pressedColor = new Color(0.2f, 0.45f, 0.75f, 1f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;
            button.onClick.AddListener(onClick);
            var text = CreateText(buttonObject.transform, label, 30, TextAnchor.MiddleCenter);
            Stretch(text.rectTransform);
        }

        Text CreateText(Transform parent, string value, int size, TextAnchor anchor)
        {
            var textObject = new GameObject(value);
            textObject.transform.SetParent(parent, false);
            var text = textObject.AddComponent<Text>();
            text.font = font;
            text.text = value;
            text.fontSize = size;
            text.alignment = anchor;
            text.color = Color.white;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            var rect = text.rectTransform;
            rect.sizeDelta = new Vector2(800f, 80f);
            return text;
        }

        static GameObject CreateGroup(Transform parent, string name)
        {
            var group = new GameObject(name);
            group.transform.SetParent(parent, false);
            Stretch(group.AddComponent<RectTransform>());
            return group;
        }

        static GameObject CreatePanel(Transform parent, string name, Vector2 position, Vector2 size)
        {
            var panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            var image = panel.AddComponent<Image>();
            image.sprite = White();
            image.color = Color.white;
            var rect = panel.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            return panel;
        }

        static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
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
