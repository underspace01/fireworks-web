using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Fireworks
{
    /// <summary>入力文字列を開花位置で発光表示する型物花火を担当する。</summary>
    public sealed class TextFireworkEffect : MonoBehaviour
    {
        private const float TEXT_EXPLOSION_DURATION = 0.3f;
        private const float TEXT_FORMATION_DURATION = 0.5f;
        private const float TEXT_DISPLAY_DURATION = 1f;
        private const float TEXT_FADE_OUT_DURATION = 0.45f;
        private const float LAUNCH_DURATION = 1.2f;
        private const float PARTICLE_SIZE = 0.14f;
        private const float LAUNCH_PARTICLE_SIZE = 0.025f;
        private const float LAUNCH_PARTICLE_LIFETIME = 0.18f;
        private const float LAUNCH_PARTICLE_RATE = 60f;
        private const float FALL_GRAVITY = 8f;
        private const float FALL_HORIZONTAL_DRIFT = 0.12f;
        private const float MINIMUM_FALL_GRAVITY_MULTIPLIER = 0.65f;
        private const float MAXIMUM_FALL_GRAVITY_MULTIPLIER = 1.35f;

        [SerializeField] private ParticleSystem _launchParticles;
        [SerializeField] private ParticleSystem _textParticles;
        [SerializeField] private TMP_FontAsset _fontAsset;

        /// <summary>文字花火Prefab生成時に子粒子を関連付ける。</summary>
        public void Configure(ParticleSystem launchParticles, ParticleSystem textParticles, TMP_FontAsset fontAsset)
        {
            _launchParticles = launchParticles;
            _textParticles = textParticles;
            _fontAsset = fontAsset;
        }

        public void Initialize(
            string text,
            Camera targetCamera,
            Vector3 launchPosition,
            Vector3 burstPosition,
            float visualScale,
            Color color,
            Material particleMaterial,
            Action onBurst,
            Action onCompleted)
        {
            if (_fontAsset == null || targetCamera == null || particleMaterial == null ||
                _launchParticles == null || _textParticles == null)
            {
                onCompleted?.Invoke();
                Destroy(gameObject);
                return;
            }

            StartCoroutine(PlayTextFirework(
                text,
                _fontAsset,
                targetCamera,
                launchPosition,
                burstPosition,
                visualScale,
                color,
                particleMaterial,
                onBurst,
                onCompleted));
        }

        private IEnumerator PlayTextFirework(
            string value,
            TMP_FontAsset fontAsset,
            Camera targetCamera,
            Vector3 launchPosition,
            Vector3 burstPosition,
            float visualScale,
            Color color,
            Material particleMaterial,
            Action onBurst,
            Action onCompleted)
        {
            transform.position = launchPosition;
            ConfigureLaunchParticles(particleMaterial, visualScale, color);
            float elapsedTime = 0f;
            while (elapsedTime < LAUNCH_DURATION)
            {
                elapsedTime += Time.deltaTime;
                transform.position = Vector3.Lerp(launchPosition, burstPosition, elapsedTime / LAUNCH_DURATION);
                yield return null;
            }

            transform.position = burstPosition;
            _launchParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            List<Vector3> glyphPositions = TextFireworkGlyphSampler.CreatePositions(
                transform, value, fontAsset, targetCamera, visualScale);
            ConfigureTextParticles(particleMaterial, visualScale, color);
            ParticleSystem.Particle[] particles = CreateParticles(glyphPositions.Count, visualScale, color);
            onBurst?.Invoke();
            yield return AnimateParticles(_textParticles, particles, glyphPositions, visualScale);

            onCompleted?.Invoke();
            Destroy(gameObject);
        }

        private void ConfigureTextParticles(Material particleMaterial, float visualScale, Color color)
        {
            _textParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystem.MainModule main = _textParticles.main;
            main.playOnAwake = false;
            main.loop = false;
            main.startLifetime = GetTotalDuration();
            main.startSpeed = 0f;
            main.startSize = PARTICLE_SIZE * visualScale;
            main.startColor = FireworkColorUtility.NormalizeIntensity(color);
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            ParticleSystem.EmissionModule emission = _textParticles.emission;
            emission.enabled = false;
            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = _textParticles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            colorOverLifetime.color = CreateFadeGradient();
            ParticleSystemRenderer renderer = _textParticles.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = particleMaterial;
            _textParticles.Play();
        }

        private void ConfigureLaunchParticles(Material particleMaterial, float visualScale, Color color)
        {
            _launchParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystem.MainModule main = _launchParticles.main;
            main.playOnAwake = false;
            main.loop = true;
            main.duration = LAUNCH_DURATION;
            main.startLifetime = LAUNCH_PARTICLE_LIFETIME;
            main.startSpeed = 0.05f;
            main.startSize = LAUNCH_PARTICLE_SIZE * visualScale;
            main.startColor = FireworkColorUtility.NormalizeIntensity(color);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            ParticleSystem.EmissionModule emission = _launchParticles.emission;
            emission.enabled = true;
            emission.rateOverTime = LAUNCH_PARTICLE_RATE;
            ParticleSystem.ShapeModule shape = _launchParticles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.01f;
            ParticleSystemRenderer renderer = _launchParticles.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = particleMaterial;
            _launchParticles.Play();
        }

        private ParticleSystem.Particle[] CreateParticles(int particleCount, float visualScale, Color color)
        {
            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[particleCount];
            float totalDuration = GetTotalDuration();
            Color particleColor = FireworkColorUtility.NormalizeIntensity(color);
            for (int index = 0; index < particles.Length; index++)
            {
                particles[index].position = Vector3.zero;
                particles[index].startColor = particleColor;
                particles[index].startSize = PARTICLE_SIZE * visualScale;
                particles[index].startLifetime = totalDuration;
                particles[index].remainingLifetime = totalDuration;
            }

            return particles;
        }

        private IEnumerator AnimateParticles(
            ParticleSystem particleSystem,
            ParticleSystem.Particle[] particles,
            List<Vector3> glyphPositions,
            float visualScale)
        {
            Vector3[] explosionPositions = new Vector3[particles.Length];
            float[] horizontalDrifts = new float[particles.Length];
            float[] fallGravityMultipliers = new float[particles.Length];
            for (int index = 0; index < particles.Length; index++)
            {
                // 爆発位置も最終文字位置への直線上に置き、途中で進行方向を変えない。
                explosionPositions[index] = glyphPositions[index] * UnityEngine.Random.Range(0.12f, 0.32f);
                horizontalDrifts[index] = UnityEngine.Random.Range(-FALL_HORIZONTAL_DRIFT, FALL_HORIZONTAL_DRIFT);
                fallGravityMultipliers[index] = UnityEngine.Random.Range(
                    MINIMUM_FALL_GRAVITY_MULTIPLIER,
                    MAXIMUM_FALL_GRAVITY_MULTIPLIER);
            }

            float elapsedTime = 0f;
            float totalDuration = GetTotalDuration();
            while (elapsedTime < totalDuration)
            {
                elapsedTime += Time.deltaTime;
                UpdateParticlePositions(
                    particles,
                    glyphPositions,
                    explosionPositions,
                    horizontalDrifts,
                    fallGravityMultipliers,
                    elapsedTime,
                    visualScale);
                particleSystem.SetParticles(particles, particles.Length);
                yield return null;
            }
        }

        private void UpdateParticlePositions(
            ParticleSystem.Particle[] particles,
            List<Vector3> glyphPositions,
            Vector3[] explosionPositions,
            float[] horizontalDrifts,
            float[] fallGravityMultipliers,
            float elapsedTime,
            float visualScale)
        {
            float formationStartTime = TEXT_EXPLOSION_DURATION;
            float formationEndTime = formationStartTime + TEXT_FORMATION_DURATION;
            float fallStartTime = formationEndTime + TEXT_DISPLAY_DURATION;
            for (int index = 0; index < particles.Length; index++)
            {
                particles[index].remainingLifetime = Mathf.Max(0f, GetTotalDuration() - elapsedTime);
                if (elapsedTime < formationStartTime)
                {
                    particles[index].position = Vector3.Lerp(
                        Vector3.zero,
                        explosionPositions[index],
                        elapsedTime / formationStartTime);
                }
                else if (elapsedTime < formationEndTime)
                {
                    float formationProgress = Mathf.InverseLerp(formationStartTime, formationEndTime, elapsedTime);
                    particles[index].position = Vector3.Lerp(
                        explosionPositions[index],
                        glyphPositions[index],
                        formationProgress);
                }
                else if (elapsedTime < fallStartTime)
                {
                    particles[index].position = glyphPositions[index];
                }
                else
                {
                    float fallTime = elapsedTime - fallStartTime;
                    float fallDistance = 0.5f * FALL_GRAVITY * fallGravityMultipliers[index] *
                        fallTime * fallTime * visualScale;
                    particles[index].position = glyphPositions[index] + new Vector3(
                        horizontalDrifts[index] * fallTime * visualScale,
                        -fallDistance,
                        0f);
                }
            }
        }

        private Gradient CreateFadeGradient()
        {
            Gradient gradient = new Gradient();
            float fadeStart = (TEXT_EXPLOSION_DURATION + TEXT_FORMATION_DURATION + TEXT_DISPLAY_DURATION) /
                GetTotalDuration();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, fadeStart),
                    new GradientColorKey(Color.black, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, fadeStart),
                    new GradientAlphaKey(0f, 1f)
                });
            return gradient;
        }


        private float GetTotalDuration()
        {
            return TEXT_EXPLOSION_DURATION + TEXT_FORMATION_DURATION + TEXT_DISPLAY_DURATION + TEXT_FADE_OUT_DURATION;
        }
    }
}
