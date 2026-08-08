using System.Collections;
using UnityEngine;

namespace Fireworks
{
    /// <summary>Prefabに置かれた粒子の再生と、選択値の反映を担当する。</summary>
    public sealed class FireworkParticleBinder : MonoBehaviour
    {
        [Header("Prefab内の粒子")]
        [SerializeField] private ParticleSystem[] _launchParticleSystems;
        [SerializeField] private ParticleSystem[] _burstParticleSystems;
        [SerializeField] private ParticleSystem[] _flashParticleSystems;

        [Header("実行時に反映する値")]
        [SerializeField, Min(0.01f)] private float _burstLifetime = 2.2f;
        [SerializeField, Range(0f, 0.5f)] private float _burstLifetimeVariationRatio = 0.12f;
        [SerializeField, Min(0f)] private float _particleSize = 0.08f;
        [SerializeField, Min(0f)] private float _burstGravityModifier = 0.025f;
        [Header("尾の消え方")]
        [SerializeField, Min(0f)] private float _trailLifetime = 0.7f;
        [SerializeField, Range(0f, 0.5f)] private float _trailLifetimeVariationRatio = 0.15f;
        [SerializeField, Range(0f, 1f)] private float _trailShortenStartNormalized = 0.65f;

        private Coroutine _trailShorteningCoroutine;

        /// <summary>Prefab作成時に、構成済みの子粒子を関連付ける。</summary>
        public void Configure(
            ParticleSystem[] launchParticleSystems,
            ParticleSystem[] burstParticleSystems,
            ParticleSystem[] flashParticleSystems)
        {
            _launchParticleSystems = launchParticleSystems;
            _burstParticleSystems = burstParticleSystems;
            _flashParticleSystems = flashParticleSystems;
        }

        public float GetBurstRadius(float burstTravelRadius, float visualScale)
        {
            return (burstTravelRadius * (1f + _burstLifetimeVariationRatio)) +
                (_particleSize * visualScale * 0.5f);
        }

        public float GetMaximumBurstLifetime(FireworkTypeDefinition effect)
        {
            return _burstLifetime * effect.BurstLifetimeMultiplier *
                (1f + _burstLifetimeVariationRatio);
        }

        public void PlayLaunch(Color color, float visualScale)
        {
            PlayParticles(_launchParticleSystems, color, visualScale, 1f);
        }

        public void StopLaunch()
        {
            StopParticles(_launchParticleSystems);
        }

        public void PlayBurst(FireworkSelection selection, float burstTravelRadius, float sizeRatio)
        {
            float lifetime = _burstLifetime * selection.Effect.BurstLifetimeMultiplier;
            float speed = burstTravelRadius * selection.Effect.BurstRadiusMultiplier / lifetime;
            float particleCountMultiplier = selection.Effect.GetParticleCountMultiplier(sizeRatio);
            PlayBurstParticles(_burstParticleSystems, selection, lifetime, speed, particleCountMultiplier);

            if (selection.Effect.UseTrails)
            {
                if (_trailShorteningCoroutine != null)
                {
                    StopCoroutine(_trailShorteningCoroutine);
                }

                _trailShorteningCoroutine = StartCoroutine(ShortenTrails(lifetime));
            }

            if (selection.Effect.CreateFlash)
            {
                PlayParticles(_flashParticleSystems, Color.white, selection.Size.VisualScale, 1f);
            }
        }

        private void PlayParticles(
            ParticleSystem[] particleSystems,
            Color color,
            float visualScale,
            float particleCountMultiplier)
        {
            if (particleSystems == null)
            {
                return;
            }

            foreach (ParticleSystem particleSystem in particleSystems)
            {
                if (particleSystem == null)
                {
                    continue;
                }

                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ParticleSystem.MainModule main = particleSystem.main;
                main.startColor = FireworkColorUtility.NormalizeIntensity(color);
                main.startSizeMultiplier = _particleSize * visualScale;
                SetBurstCount(particleSystem, particleCountMultiplier);
                particleSystem.Play(true);
            }
        }

        private void PlayBurstParticles(
            ParticleSystem[] particleSystems,
            FireworkSelection selection,
            float lifetime,
            float speed,
            float particleCountMultiplier)
        {
            if (particleSystems == null)
            {
                return;
            }

            foreach (ParticleSystem particleSystem in particleSystems)
            {
                if (particleSystem == null)
                {
                    continue;
                }

                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ParticleSystem.MainModule main = particleSystem.main;
                main.startColor = selection.Effect.ParticleFade == FireworkParticleFade.ChangeColor
                    ? Color.white
                    : FireworkColorUtility.NormalizeIntensity(selection.Color);
                main.startLifetime = new ParticleSystem.MinMaxCurve(
                    lifetime * (1f - _burstLifetimeVariationRatio),
                    lifetime * (1f + _burstLifetimeVariationRatio));
                main.startSpeed = speed;
                main.startSizeMultiplier = _particleSize * selection.Size.VisualScale;
                // multiplierプロパティは基準値への乗算ではなく値自体を上書きする。
                main.gravityModifier = _burstGravityModifier * selection.Effect.GravityMultiplier;
                ConfigureBurstColor(particleSystem, selection);
                SetBurstCount(particleSystem, particleCountMultiplier);
                particleSystem.Play(true);
            }
        }

        private void SetBurstCount(ParticleSystem particleSystem, float multiplier)
        {
            ParticleSystem.EmissionModule emission = particleSystem.emission;
            ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[emission.burstCount];
            emission.GetBursts(bursts);
            for (int index = 0; index < bursts.Length; index++)
            {
                ParticleSystem.MinMaxCurve count = bursts[index].count;
                bursts[index].count = new ParticleSystem.MinMaxCurve(
                    count.constant * multiplier);
            }

            emission.SetBursts(bursts);
        }

        private void ConfigureBurstColor(ParticleSystem particleSystem, FireworkSelection selection)
        {
            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particleSystem.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Color primaryColor = FireworkColorUtility.NormalizeIntensity(selection.Color);
            if (selection.Effect.ParticleFade == FireworkParticleFade.ChangeColor)
            {
                Color changeColor = FireworkColorUtility.NormalizeIntensity(selection.ChangeColor);
                colorOverLifetime.color = CreateGradient(
                    new[]
                    {
                        new GradientColorKey(primaryColor, 0f),
                        new GradientColorKey(primaryColor, 0.55f),
                        new GradientColorKey(changeColor, 0.78f),
                        new GradientColorKey(Color.black, 1f)
                    },
                    0.82f);
                return;
            }

            colorOverLifetime.color = CreateGradient(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 0.82f),
                    new GradientColorKey(Color.black, 1f)
                },
                0.82f);
        }

        private Gradient CreateGradient(GradientColorKey[] colorKeys, float fadeStart)
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                colorKeys,
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, fadeStart),
                    new GradientAlphaKey(0f, 1f)
                });
            return gradient;
        }

        private IEnumerator ShortenTrails(float particleLifetime)
        {
            float shorteningStartTime = particleLifetime * _trailShortenStartNormalized;
            float elapsedTime = 0f;
            SetTrailLifetime(1f);

            while (elapsedTime < particleLifetime)
            {
                elapsedTime += Time.deltaTime;
                if (elapsedTime >= shorteningStartTime)
                {
                    float shorteningProgress = Mathf.InverseLerp(
                        shorteningStartTime,
                        particleLifetime,
                        elapsedTime);
                    SetTrailLifetime(1f - shorteningProgress);
                }

                yield return null;
            }

            SetTrailLifetime(0f);
            _trailShorteningCoroutine = null;
        }

        private void SetTrailLifetime(float multiplier)
        {
            if (_burstParticleSystems == null)
            {
                return;
            }

            float minimumLifetime = _trailLifetime * (1f - _trailLifetimeVariationRatio) * multiplier;
            float maximumLifetime = _trailLifetime * (1f + _trailLifetimeVariationRatio) * multiplier;
            foreach (ParticleSystem particleSystem in _burstParticleSystems)
            {
                if (particleSystem == null)
                {
                    continue;
                }

                ParticleSystem.TrailModule trails = particleSystem.trails;
                if (trails.enabled)
                {
                    trails.lifetime = new ParticleSystem.MinMaxCurve(minimumLifetime, maximumLifetime);
                }
            }
        }

        private void StopParticles(ParticleSystem[] particleSystems)
        {
            if (particleSystems == null)
            {
                return;
            }

            foreach (ParticleSystem particleSystem in particleSystems)
            {
                if (particleSystem != null)
                {
                    particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
            }
        }

    }
}
