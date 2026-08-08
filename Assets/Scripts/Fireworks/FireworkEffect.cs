using System;
using System.Collections;
using UnityEngine;

namespace Fireworks
{
    /// <summary>花火Prefabの再生順と完了通知だけを担当する。</summary>
    public sealed class FireworkEffect : MonoBehaviour
    {
        [SerializeField, Min(0.01f)]
        private float _launchDuration = 1.2f;

        [SerializeField, Min(0f)]
        private float _destroyDelay = 3f;

        [SerializeField, Min(0f)]
        private float _burstRadiusPadding = 0.05f;

        [SerializeField]
        private FireworkFlight _flight;

        [SerializeField]
        private FireworkParticleBinder _particleBinder;

        private Action _onCompleted;
        private bool _isCompleted;

        public float LaunchDuration => _launchDuration;

        public void Initialize(
            FireworkSelection selection,
            Vector3 burstPosition,
            float burstTravelRadius,
            FireworkAudio fireworkAudio,
            float sizeRatio,
            Action onCompleted)
        {
            if (_flight == null || _particleBinder == null)
            {
                onCompleted?.Invoke();
                Destroy(gameObject);
                return;
            }

            _onCompleted = onCompleted;
            StartCoroutine(PlayFirework(
                selection,
                burstPosition,
                burstTravelRadius,
                fireworkAudio,
                sizeRatio));
        }

        public float GetBurstRadius(float burstTravelRadius, float visualScale)
        {
            return _particleBinder == null
                ? burstTravelRadius
                : _particleBinder.GetBurstRadius(burstTravelRadius, visualScale) + _burstRadiusPadding;
        }

        private IEnumerator PlayFirework(
            FireworkSelection selection,
            Vector3 burstPosition,
            float burstTravelRadius,
            FireworkAudio fireworkAudio,
            float sizeRatio)
        {
            _particleBinder.PlayLaunch(selection.Color, selection.Size.VisualScale);
            yield return _flight.Travel(burstPosition, _launchDuration);
            _particleBinder.StopLaunch();
            _particleBinder.PlayBurst(selection, burstTravelRadius, sizeRatio);

            if (fireworkAudio != null)
            {
                fireworkAudio.PlayExplosion(sizeRatio);
            }

            yield return new WaitForSeconds(Mathf.Max(
                _destroyDelay,
                _particleBinder.GetMaximumBurstLifetime(selection.Effect)));
            Complete();
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            Complete();
        }

        private void Complete()
        {
            if (_isCompleted)
            {
                return;
            }

            _isCompleted = true;
            _onCompleted?.Invoke();
            _onCompleted = null;
        }
    }
}
