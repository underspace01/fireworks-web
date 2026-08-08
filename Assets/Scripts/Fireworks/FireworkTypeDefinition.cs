using UnityEngine;

namespace Fireworks
{
    /// <summary>
    /// 花火種類Prefabに付与する、選択UIと演出制御の設定。
    /// </summary>
    public sealed class FireworkTypeDefinition : MonoBehaviour
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private FireworkEffect _effect;
        [SerializeField] private TextFireworkEffect _textEffect;
        [SerializeField] private FireworkLaunchType _launchType;
        [SerializeField] private bool _allowsChangeColor;
        [Header("開花")]
        [SerializeField, Min(0.01f)] private float _burstLifetimeMultiplier = 1f;
        [SerializeField, Min(0f)] private float _gravityMultiplier = 1f;
        [SerializeField, Min(0f)] private float _burstRadiusMultiplier = 1f;
        [Header("サイズ別粒子数")]
        [SerializeField, Min(0f)] private float _minimumParticleCountMultiplier = 1f;
        [SerializeField, Min(0f)] private float _maximumParticleCountMultiplier = 2.5f;
        [Header("追加演出")]
        [SerializeField] private bool _useTrails;
        [SerializeField] private FireworkParticleFade _particleFade;
        [SerializeField] private bool _useNoise;
        [SerializeField, Min(0f)] private float _noiseStrengthMultiplier = 1f;
        [SerializeField, Min(0f)] private float _noiseFrequencyMultiplier = 1f;
        [SerializeField] private bool _createFlash;

        public string Id => _id;
        public string DisplayName => _displayName;
        public FireworkEffect Effect => _effect;
        public TextFireworkEffect TextEffect => _textEffect;
        public FireworkLaunchType LaunchType => _launchType;
        public bool IsTextFirework => _launchType == FireworkLaunchType.Text;
        public bool AllowsChangeColor => _allowsChangeColor;
        public float BurstLifetimeMultiplier => _burstLifetimeMultiplier;
        public float GravityMultiplier => _gravityMultiplier;
        public float BurstRadiusMultiplier => _burstRadiusMultiplier;
        public bool UseTrails => _useTrails;
        public FireworkParticleFade ParticleFade => _particleFade;
        public bool UseNoise => _useNoise;
        public float NoiseStrengthMultiplier => _noiseStrengthMultiplier;
        public float NoiseFrequencyMultiplier => _noiseFrequencyMultiplier;
        public bool CreateFlash => _createFlash;

        public float GetParticleCountMultiplier(float sizeRatio)
        {
            return Mathf.Lerp(_minimumParticleCountMultiplier, _maximumParticleCountMultiplier, sizeRatio);
        }

        public void ConfigureTextFirework(TextFireworkEffect textEffect)
        {
            _id = "Text";
            _displayName = "文字花火";
            _textEffect = textEffect;
            _launchType = FireworkLaunchType.Text;
            _allowsChangeColor = false;
        }
    }

    public enum FireworkParticleFade
    {
        None,
        FadeOut,
        ChangeColor
    }

    public enum FireworkLaunchType { Particle, Text }
}
