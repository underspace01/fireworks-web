using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Fireworks
{
    /// <summary>実行時UI全体の生成と表示状態だけを調停する。</summary>
    public sealed class FireworkDebugUI : MonoBehaviour
    {
        [SerializeField] private FireworkLauncher _launcher;
        [SerializeField] private FireworkSettings _settings;
        [SerializeField] private TMP_FontAsset _notoSansFontAsset;
        [SerializeField] private Vector2 _panelSize = new Vector2(360f, 420f);
        [SerializeField] private float _margin = 24f;
        [SerializeField] private float _rowHeight = 44f;
        [SerializeField, Min(1f)] private float _settingsButtonSize = 56f;
        [SerializeField, Min(1f)] private float _settingsIconSize = 24f;
        [SerializeField, Range(0f, 1f)] private float _settingsIconOpacity = 0.45f;
        [SerializeField] private Key _toggleKey = Key.Slash;
        [SerializeField] private Key _infoToggleKey = Key.Escape;

        private static RectTransform _activePanel;
        private static TMP_FontAsset _fallbackJapaneseFont;
        private static bool _isModalPanelVisible;

        private GameObject _canvasObject;
        private FireworkParameterPanel _parameterPanel;
        private ApplicationInfoPanel _infoPanel;
        private bool _restoreParameterPanel;

        public static bool IsPanelVisible => _isModalPanelVisible ||
            _activePanel != null && _activePanel.gameObject.activeInHierarchy;

        private void Awake()
        {
            if (_launcher == null || _settings == null ||
                _settings.SizeCount == 0 || _settings.EffectCount == 0 || _settings.ColorCount == 0)
            {
                enabled = false;
                return;
            }

            CreateEventSystem();
            RectTransform canvasRoot = CreateCanvas();
            TMP_FontAsset font = GetJapaneseFont();
            CreateLaunchSurface(canvasRoot);
            _parameterPanel = new FireworkParameterPanel(
                canvasRoot, _launcher, _settings, _panelSize, _margin, _rowHeight, font, HandleInfoKey);
            _infoPanel = new ApplicationInfoPanel(canvasRoot, _rowHeight, font, CloseInfoPanel);
            _activePanel = _parameterPanel.RectTransform;
            CreateSettingsButton(canvasRoot);
            SetParameterVisible(false);
        }

        private void Update()
        {
            if (Keyboard.current == null)
            {
                return;
            }

            var toggleKey = Keyboard.current[_toggleKey];
            if (toggleKey != null && toggleKey.wasPressedThisFrame)
            {
                SetParameterVisible(!_canvasObject.activeSelf || !_parameterPanel.IsVisible);
            }

            var infoToggleKey = Keyboard.current[_infoToggleKey];
            if (infoToggleKey != null && infoToggleKey.wasPressedThisFrame)
            {
                HandleInfoKey();
            }
        }

        private void OnDestroy()
        {
            if (_activePanel != null && _activePanel.transform.IsChildOf(transform))
            {
                _activePanel = null;
            }

            _isModalPanelVisible = false;
            Cursor.visible = true;
        }

        public static bool IsPointerOverUi()
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return true;
            }

            return _isModalPanelVisible ||
                _activePanel != null && _activePanel.gameObject.activeInHierarchy && Mouse.current != null &&
                RectTransformUtility.RectangleContainsScreenPoint(_activePanel, Mouse.current.position.ReadValue());
        }

        public void ToggleVisibility()
        {
            SetParameterVisible(!_canvasObject.activeSelf || !_parameterPanel.IsVisible);
        }

        private void HandleInfoKey()
        {
            if (_infoPanel.IsLicenseVisible)
            {
                _infoPanel.CloseLicense();
                return;
            }

            if (_infoPanel.IsVisible)
            {
                CloseInfoPanel();
                return;
            }

            _canvasObject.SetActive(true);
            _restoreParameterPanel = _parameterPanel.IsVisible;
            _parameterPanel.SetVisible(false);
            _infoPanel.Show();
            _isModalPanelVisible = true;
            Cursor.visible = true;
        }

        private void CloseInfoPanel()
        {
            _infoPanel.Hide();
            _parameterPanel.SetVisible(_restoreParameterPanel);
            _isModalPanelVisible = false;
            if (!_parameterPanel.IsVisible)
            {
                _canvasObject.SetActive(true);
                Cursor.visible = true;
            }
        }

        private void SetParameterVisible(bool isVisible)
        {
            _infoPanel.HideAll();
            _isModalPanelVisible = false;
            _parameterPanel.SetVisible(isVisible);
            _canvasObject.SetActive(true);
            Cursor.visible = true;
        }

        private void CreateLaunchSurface(RectTransform canvasRoot)
        {
            var surface = RuntimeUiFactory.CreateObject("Launch Surface", canvasRoot);
            var rect = surface.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            surface.AddComponent<UnityEngine.UI.Image>().color = Color.clear;
            // UIイベントに統一し、タッチと互換マウス入力の二重打上を防ぐ。
            surface.AddComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
            {
                if (!IsPanelVisible) _launcher.LaunchRandomFirework(_parameterPanel.TextValue);
            });
        }

        private void CreateSettingsButton(RectTransform canvasRoot)
        {
            GameObject buttonObject = RuntimeUiFactory.CreateObject("Settings Button", canvasRoot);
            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(1f, 0f);
            buttonRect.anchorMax = buttonRect.anchorMin;
            buttonRect.pivot = buttonRect.anchorMin;
            buttonRect.sizeDelta = Vector2.one * _settingsButtonSize;
            buttonRect.anchoredPosition = new Vector2(-_margin, _margin);
            // 見た目は小さく保ち、透明な領域で指のタップを受け付ける。
            buttonObject.AddComponent<UnityEngine.UI.Image>().color = Color.clear;
            var button = buttonObject.AddComponent<UnityEngine.UI.Button>();
            button.onClick.AddListener(ToggleVisibility);

            GameObject iconObject = RuntimeUiFactory.CreateObject("Settings Icon", buttonRect);
            RectTransform iconRect = iconObject.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = iconRect.anchorMin;
            iconRect.sizeDelta = Vector2.one * Mathf.Min(_settingsIconSize, _settingsButtonSize);
            var icon = iconObject.AddComponent<SettingsIconGraphic>();
            icon.raycastTarget = false;
            // 透明なタップ領域の色遷移をアイコンへ引き継がず、常に見つけられる濃さに保つ。
            button.transition = Selectable.Transition.None;
            icon.color = new Color(1f, 1f, 1f, _settingsIconOpacity);
        }

        private void CreateEventSystem()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystemObject.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        private RectTransform CreateCanvas()
        {
            _canvasObject = new GameObject("Firework Parameter Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            _canvasObject.transform.SetParent(transform, false);
            _canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler canvasScaler = _canvasObject.GetComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(480f, 480f);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            return _canvasObject.GetComponent<RectTransform>();
        }

        private TMP_FontAsset GetJapaneseFont()
        {
            if (_fallbackJapaneseFont == null)
            {
                _fallbackJapaneseFont = _notoSansFontAsset;
            }

            return _fallbackJapaneseFont != null ? _fallbackJapaneseFont : TMP_Settings.defaultFontAsset;
        }
    }
}
