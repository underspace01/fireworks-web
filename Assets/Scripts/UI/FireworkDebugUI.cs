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
        [SerializeField] private float _rowHeight = 36f;
        [SerializeField] private Key _toggleKey = Key.Slash;
        [SerializeField] private Key _infoToggleKey = Key.Escape;

        private static RectTransform _activePanel;
        private static TMP_FontAsset _fallbackJapaneseFont;
        private static bool _isModalPanelVisible;

        private GameObject _canvasObject;
        private FireworkParameterPanel _parameterPanel;
        private ApplicationInfoPanel _infoPanel;

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
            _parameterPanel = new FireworkParameterPanel(
                canvasRoot, _launcher, _settings, _panelSize, _margin, _rowHeight, font);
            _infoPanel = new ApplicationInfoPanel(canvasRoot, _rowHeight, font, CloseInfoPanel);
            _activePanel = _parameterPanel.RectTransform;
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
            _infoPanel.Show();
            _isModalPanelVisible = true;
            Cursor.visible = true;
        }

        private void CloseInfoPanel()
        {
            _infoPanel.Hide();
            _isModalPanelVisible = false;
            if (!_parameterPanel.IsVisible)
            {
                _canvasObject.SetActive(false);
                Cursor.visible = false;
            }
        }

        private void SetParameterVisible(bool isVisible)
        {
            _infoPanel.HideAll();
            _isModalPanelVisible = false;
            _parameterPanel.SetVisible(isVisible);
            _canvasObject.SetActive(isVisible);
            Cursor.visible = isVisible;
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
            canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasScaler.matchWidthOrHeight = 1f;
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
