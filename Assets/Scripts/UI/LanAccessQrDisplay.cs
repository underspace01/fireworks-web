using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.Common;

namespace Fireworks
{
    /// <summary>
    /// 待機中だけLAN操作画面へのQRコードを右下に表示する。
    /// </summary>
    public sealed class LanAccessQrDisplay : MonoBehaviour
    {
        [SerializeField] private LanWebServer _webServer;
        [SerializeField] private FireworkLauncher _launcher;
        [SerializeField, Min(0f)] private float _showDelayAfterCompleted = 3f;
        [SerializeField, Min(64)] private int _qrDisplaySize = 296;
        [SerializeField, Min(0f)] private float _margin = 24f;
        [SerializeField, Min(1)] private int _modulePixelSize = 8;

        private GameObject _canvasObject;
        private Coroutine _showCoroutine;

        private void Awake()
        {
            if (_webServer == null || _launcher == null)
            {
                enabled = false;
                return;
            }

            CreateDisplay();
        }

        private void OnEnable()
        {
            if (_launcher == null)
            {
                return;
            }

            _launcher.FireworkStarted += HideImmediately;
            _launcher.FireworksCompleted += ShowAfterDelay;
        }

        private void Start()
        {
            if (!_launcher.HasActiveFireworks)
            {
                ShowImmediately();
            }
        }

        private void OnDisable()
        {
            if (_launcher != null)
            {
                _launcher.FireworkStarted -= HideImmediately;
                _launcher.FireworksCompleted -= ShowAfterDelay;
            }
        }

        private void OnDestroy()
        {
            if (_canvasObject != null)
            {
                Destroy(_canvasObject);
            }
        }

        private void CreateDisplay()
        {
            _canvasObject = new GameObject("LAN Access QR Canvas", typeof(Canvas), typeof(CanvasScaler));
            Canvas canvas = _canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            CanvasScaler scaler = _canvasObject.GetComponent<CanvasScaler>();
            // QRの1マスを整数ピクセルで描画し、カメラで読み取れる輪郭を維持する。
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;

            GameObject qrObject = new GameObject("LAN Access QR", typeof(RectTransform), typeof(RawImage));
            qrObject.transform.SetParent(_canvasObject.transform, false);
            RectTransform rectTransform = qrObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(1f, 0f);
            rectTransform.anchorMax = new Vector2(1f, 0f);
            rectTransform.pivot = new Vector2(1f, 0f);
            rectTransform.anchoredPosition = new Vector2(-_margin, _margin);
            int qrTextureSize = (29 + (4 * 2)) * _modulePixelSize;
            float displaySize = Mathf.Max(_qrDisplaySize, qrTextureSize);
            rectTransform.sizeDelta = new Vector2(displaySize, displaySize);

            RawImage image = qrObject.GetComponent<RawImage>();
            image.raycastTarget = false;
            if (!_webServer.IsRunning || !TryCreateTexture(out Texture2D texture))
            {
                _canvasObject.SetActive(false);
                return;
            }

            image.texture = texture;
            _canvasObject.SetActive(false);
        }

        private bool TryCreateTexture(out Texture2D texture)
        {
            texture = null;
            if (string.IsNullOrEmpty(_webServer.AccessUrl))
            {
                return false;
            }

            int textureSize = (29 + (4 * 2)) * _modulePixelSize;
            BarcodeWriter writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new EncodingOptions
                {
                    Width = textureSize,
                    Height = textureSize,
                    Margin = 4
                }
            };
            Color32[] pixels = writer.Write(_webServer.AccessUrl);
            texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = "LanAccessQrCode"
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return true;
        }

        private void HideImmediately()
        {
            if (_showCoroutine != null)
            {
                StopCoroutine(_showCoroutine);
                _showCoroutine = null;
            }

            if (_canvasObject != null)
            {
                _canvasObject.SetActive(false);
            }
        }

        private void ShowAfterDelay()
        {
            if (_showCoroutine != null)
            {
                StopCoroutine(_showCoroutine);
            }

            _showCoroutine = StartCoroutine(ShowAfterDelayCoroutine());
        }

        private IEnumerator ShowAfterDelayCoroutine()
        {
            yield return new WaitForSeconds(_showDelayAfterCompleted);
            _showCoroutine = null;
            if (!_launcher.HasActiveFireworks)
            {
                ShowImmediately();
            }
        }

        private void ShowImmediately()
        {
            if (_canvasObject != null && _webServer.IsRunning && !_launcher.HasActiveFireworks)
            {
                _canvasObject.SetActive(true);
            }
        }
    }
}
