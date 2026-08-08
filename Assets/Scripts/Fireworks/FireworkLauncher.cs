using UnityEngine;
using UnityEngine.InputSystem;

namespace Fireworks
{
    public sealed class FireworkLauncher : MonoBehaviour
    {
        private const string RANDOM_TEXT_CHARACTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        [SerializeField]
        private FireworkSettings _settings;

        [SerializeField]
        private Camera _targetCamera;

        [SerializeField]
        private FireworkAudio _fireworkAudio;

        [SerializeField]
        private Material _textParticleMaterial;

        [SerializeField, Range(0f, 1f)]
        private float _minimumLaunchViewportX = 0.1f;

        [SerializeField, Range(0f, 1f)]
        private float _maximumLaunchViewportX = 0.9f;

        [SerializeField, Range(0f, 1f)]
        private float _launchViewportY = 0f;

        private int _activeFireworkCount;

        public bool HasActiveFireworks => _activeFireworkCount > 0;

        public event System.Action FireworkStarted;
        public event System.Action FireworksCompleted;

        private void Awake()
        {
            if (_targetCamera == null)
            {
                _targetCamera = Camera.main;
            }
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                LaunchFirework();
            }

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame &&
                !FireworkDebugUI.IsPointerOverUi())
            {
                LaunchFirework();
            }
        }

        public void LaunchFirework()
        {
            LaunchRandomFirework(null);
        }

        public void LaunchRandomFirework(string text)
        {
            if (_settings == null || !_settings.TryCreateRandomSelection(out FireworkSelection selection)) return;
            if (selection.Effect.IsTextFirework)
            {
                LaunchRandomTextFirework(text);
                return;
            }
            LaunchFirework(selection);
        }

        public void LaunchFirework(int sizeIndex, int structureIndex, int colorIndex, int changeColorIndex)
        {
            if (_settings == null || _targetCamera == null ||
                !_settings.TryCreateSelection(sizeIndex, structureIndex, colorIndex, changeColorIndex, out FireworkSelection selection))
            {
                return;
            }

            LaunchFirework(selection);
        }

        public void LaunchTextFirework(string text, int sizeIndex, int colorIndex)
        {
            if (string.IsNullOrWhiteSpace(text) || text.Length > 6 || _settings == null || _targetCamera == null ||
                sizeIndex < 0 || sizeIndex >= _settings.SizeCount || colorIndex < 0 || colorIndex >= _settings.ColorCount)
            {
                return;
            }

            if (!_settings.TryCreateTextSelection(sizeIndex, colorIndex, out FireworkSelection selection) ||
                selection.Effect.TextEffect == null)
            {
                return;
            }

            FireworkSizeDefinition size = selection.Size;
            Color color = selection.Color;
            float sizeRatio = _settings.GetSizeNormalized(size);
            float viewportX = Random.Range(_minimumLaunchViewportX, _maximumLaunchViewportX);
            Vector3 launchPosition = GetViewportPosition(viewportX, _launchViewportY);
            float burstViewportY = Mathf.Clamp(_settings.GetLaunchHeightNormalized(
                _settings.GetRandomLaunchHeight(size)), 0.2f, 0.8f);
            Vector3 burstPosition = GetViewportPosition(viewportX, burstViewportY);

            if (_fireworkAudio != null)
            {
                _fireworkAudio.PlayLaunch(sizeRatio, 1.2f);
            }

            TextFireworkEffect textFirework = Instantiate(selection.Effect.TextEffect);
            _activeFireworkCount++;
            FireworkStarted?.Invoke();
            textFirework.Initialize(
                text.Trim(),
                _targetCamera,
                launchPosition,
                burstPosition,
                size.VisualScale,
                color,
                _textParticleMaterial,
                () => OnTextFireworkBurst(sizeRatio),
                OnFireworkCompleted);
        }

        public void LaunchRandomTextFirework(string text)
        {
            if (_settings == null || _settings.SizeCount == 0 || _settings.ColorCount == 0)
            {
                return;
            }

            string launchText = string.IsNullOrWhiteSpace(text)
                ? RANDOM_TEXT_CHARACTERS[Random.Range(0, RANDOM_TEXT_CHARACTERS.Length)].ToString()
                : text.Trim();
            LaunchTextFirework(
                launchText,
                Random.Range(0, _settings.SizeCount),
                Random.Range(0, _settings.ColorCount));
        }

        private void LaunchFirework(FireworkSelection selection)
        {
            if (selection.Effect == null)
            {
                return;
            }

            FireworkEffect effectPrefab = selection.Effect.Effect;
            if (effectPrefab == null)
            {
                return;
            }

            float burstTravelRadius = GetBurstTravelRadius(selection.Size.BurstDiameter);
            float burstRadius = effectPrefab.GetBurstRadius(burstTravelRadius, selection.Size.VisualScale) *
                selection.Effect.BurstRadiusMultiplier;
            float horizontalMargin = GetHorizontalViewportMargin(burstRadius);
            float verticalMargin = GetVerticalViewportMargin(burstRadius);
            float minimumViewportX = Mathf.Max(_minimumLaunchViewportX, horizontalMargin);
            float maximumViewportX = Mathf.Min(_maximumLaunchViewportX, 1f - horizontalMargin);
            float viewportX = minimumViewportX <= maximumViewportX
                ? Random.Range(minimumViewportX, maximumViewportX)
                : 0.5f;
            Vector3 launchPosition = GetViewportPosition(viewportX, _launchViewportY);
            float launchHeight = _settings.GetRandomLaunchHeight(selection.Size);
            float heightRatio = _settings.GetLaunchHeightNormalized(launchHeight);
            float burstViewportY = heightRatio;
            burstViewportY = Mathf.Clamp(burstViewportY, verticalMargin, 1f - verticalMargin);
            Vector3 burstPosition = GetViewportPosition(viewportX, burstViewportY);
            float sizeRatio = _settings.GetSizeNormalized(selection.Size);
            if (_fireworkAudio != null)
            {
                _fireworkAudio.PlayLaunch(sizeRatio, effectPrefab.LaunchDuration);
            }

            FireworkEffect firework = Instantiate(effectPrefab, launchPosition, Quaternion.identity);
            _activeFireworkCount++;
            FireworkStarted?.Invoke();
            firework.Initialize(
                selection,
                burstPosition,
                burstTravelRadius,
                _fireworkAudio,
                sizeRatio,
                OnFireworkCompleted);
        }

        private void OnFireworkCompleted()
        {
            _activeFireworkCount = Mathf.Max(0, _activeFireworkCount - 1);
            if (_activeFireworkCount == 0)
            {
                FireworksCompleted?.Invoke();
            }
        }

        private void OnTextFireworkBurst(float sizeRatio)
        {
            if (_fireworkAudio != null)
            {
                _fireworkAudio.PlayExplosion(sizeRatio);
            }
        }

        private float GetBurstTravelRadius(float burstDiameter)
        {
            float screenHeight = _settings.ScreenHeight;
            float viewportHeight = _targetCamera.orthographicSize * 2f;
            if (screenHeight <= 0f || viewportHeight <= 0f)
            {
                return 0f;
            }

            return (burstDiameter / screenHeight) * viewportHeight * 0.5f;
        }

        private Vector3 GetViewportPosition(float viewportX, float viewportY)
        {
            float distance = Mathf.Abs(_targetCamera.transform.position.z);
            return _targetCamera.ViewportToWorldPoint(new Vector3(viewportX, viewportY, distance));
        }

        private float GetHorizontalViewportMargin(float burstRadius)
        {
            float viewportWidth = _targetCamera.orthographicSize * _targetCamera.aspect * 2f;
            return burstRadius / viewportWidth;
        }

        private float GetVerticalViewportMargin(float burstRadius)
        {
            float viewportHeight = _targetCamera.orthographicSize * 2f;
            return burstRadius / viewportHeight;
        }
    }
}
