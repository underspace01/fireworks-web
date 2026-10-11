using System;
using UnityEngine;

namespace Fireworks
{
    [CreateAssetMenu(fileName = "FireworkSettings", menuName = "Fireworks/Firework Settings")]
    public sealed class FireworkSettings : ScriptableObject
    {
        [SerializeField]
        private FireworkSizeDefinition[] _sizes;

        [SerializeField]
        private FireworkTypeDefinition[] _effects;

        [SerializeField]
        private FireworkColorDefinition[] _colors;


        [SerializeField, Range(0f, 1f)]
        private float _launchHeightVariationRatio = 0.05f;

        [Header("表示高度")]
        [SerializeField, Min(0f)]
        private float _screenBottomHeight = 50f;

        [SerializeField, Min(0f)]
        private float _screenTopHeight = 450f;

        public bool TryCreateRandomSelection(out FireworkSelection selection)
        {
            selection = default;
            if (SizeCount == 0) return false;
            return TryCreateRandomSelection(UnityEngine.Random.Range(0, SizeCount), out selection);
        }

        public bool TryCreateRandomSelection(int sizeIndex, out FireworkSelection selection)
        {
            selection = default;

            if (!IsValidIndex(sizeIndex, SizeCount) ||
                _effects == null || _effects.Length == 0 ||
                _colors == null || _colors.Length == 0)
            {
                return false;
            }

            FireworkSizeDefinition size = _sizes[sizeIndex];
            FireworkTypeDefinition effect = _effects[UnityEngine.Random.Range(0, _effects.Length)];
            FireworkColorDefinition color = _colors[UnityEngine.Random.Range(0, _colors.Length)];
            if (effect == null)
            {
                return false;
            }

            Color changeColor = color.Value;

            if (effect.AllowsChangeColor)
            {
                changeColor = _colors[UnityEngine.Random.Range(0, _colors.Length)].Value;
            }

            selection = new FireworkSelection(size, effect, color.Value, changeColor);
            return true;
        }

        public int SizeCount => _sizes?.Length ?? 0;
        public int EffectCount => _effects?.Length ?? 0;
        public int ColorCount => _colors?.Length ?? 0;
        public float ScreenHeight => _screenTopHeight - _screenBottomHeight;

        public int GetClosestSizeIndex(float viewportHeight)
        {
            float targetHeight = Mathf.Lerp(_screenBottomHeight, _screenTopHeight, Mathf.Clamp01(viewportHeight));
            int closestIndex = -1;
            float closestDistance = float.PositiveInfinity;
            for (int index = 0; index < SizeCount; index++)
            {
                float distance = Mathf.Abs(_sizes[index].LaunchHeight - targetHeight);
                // 境界では低い高度の玉を選び、候補の並び順に依存させない。
                if (distance < closestDistance || distance == closestDistance &&
                    closestIndex >= 0 && _sizes[index].LaunchHeight < _sizes[closestIndex].LaunchHeight)
                {
                    closestIndex = index;
                    closestDistance = distance;
                }
            }
            return closestIndex;
        }
        public bool TryCreateTextSelection(int sizeIndex, int colorIndex, out FireworkSelection selection)
        {
            selection = default;
            for (int index = 0; index < EffectCount; index++)
            {
                if (_effects[index] != null && _effects[index].IsTextFirework)
                {
                    return TryCreateSelection(sizeIndex, index, colorIndex, colorIndex, out selection);
                }
            }
            return false;
        }

        public void RegisterEffect(FireworkTypeDefinition effect)
        {
            if (effect == null) return;
            if (_effects == null) _effects = Array.Empty<FireworkTypeDefinition>();
            if (Array.IndexOf(_effects, effect) >= 0) return;
            Array.Resize(ref _effects, EffectCount + 1);
            _effects[EffectCount - 1] = effect;
        }

        public float GetSizeNormalized(FireworkSizeDefinition size)
        {
            if (_sizes == null || _sizes.Length == 0)
            {
                return 0f;
            }

            float minimumDiameter = _sizes[0].BurstDiameter;
            float maximumDiameter = minimumDiameter;
            for (int index = 1; index < _sizes.Length; index++)
            {
                float burstDiameter = _sizes[index].BurstDiameter;
                minimumDiameter = Mathf.Min(minimumDiameter, burstDiameter);
                maximumDiameter = Mathf.Max(maximumDiameter, burstDiameter);
            }

            return Mathf.InverseLerp(minimumDiameter, maximumDiameter, size.BurstDiameter);
        }

        public FireworkSizeDefinition GetSize(int index)
        {
            return _sizes[index];
        }

        public FireworkTypeDefinition GetEffect(int index)
        {
            return _effects[index];
        }

        public FireworkColorDefinition GetColor(int index)
        {
            return _colors[index];
        }

        public bool TryGetSizeIndex(string id, out int result)
        {
            result = FindSizeIndex(id);
            return result >= 0;
        }

        public bool TryGetEffectIndex(string typeName, out int result)
        {
            result = FindStructureIndex(typeName);
            return result >= 0;
        }

        public bool TryGetColorIndex(string id, out int result)
        {
            result = FindColorIndex(id);
            return result >= 0;
        }

        public bool TryCreateSelection(
            int sizeIndex,
            int structureIndex,
            int colorIndex,
            int changeColorIndex,
            out FireworkSelection selection)
        {
            selection = default;

            if (!IsValidIndex(sizeIndex, SizeCount) ||
                !IsValidIndex(structureIndex, EffectCount) ||
                !IsValidIndex(colorIndex, ColorCount) ||
                !IsValidIndex(changeColorIndex, ColorCount) ||
                _effects[structureIndex] == null)
            {
                return false;
            }

            FireworkSizeDefinition size = _sizes[sizeIndex];
            FireworkTypeDefinition effect = _effects[structureIndex];
            FireworkColorDefinition color = _colors[colorIndex];
            FireworkColorDefinition changeColor = _colors[changeColorIndex];
            selection = new FireworkSelection(size, effect, color.Value, changeColor.Value);
            return true;
        }

        public float GetRandomLaunchHeight(FireworkSizeDefinition size)
        {
            float minimumMultiplier = 1f - _launchHeightVariationRatio;
            float maximumMultiplier = 1f + _launchHeightVariationRatio;
            return size.LaunchHeight * UnityEngine.Random.Range(minimumMultiplier, maximumMultiplier);
        }

        public float GetLaunchHeightNormalized(float launchHeight)
        {
            if (_screenTopHeight <= _screenBottomHeight)
            {
                return 0f;
            }

            return Mathf.InverseLerp(_screenBottomHeight, _screenTopHeight, launchHeight);
        }

        private bool IsValidIndex(int index, int count)
        {
            return index >= 0 && index < count;
        }

        private int FindSizeIndex(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return -1;
            }

            for (int index = 0; index < SizeCount; index++)
            {
                if (_sizes[index].Id == id)
                {
                    return index;
                }
            }

            return -1;
        }

        private int FindStructureIndex(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return -1;
            }

            for (int index = 0; index < EffectCount; index++)
            {
                if (_effects[index] != null && _effects[index].Id == typeName)
                {
                    return index;
                }
            }

            return -1;
        }

        private int FindColorIndex(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return -1;
            }

            for (int index = 0; index < ColorCount; index++)
            {
                if (_colors[index].Id == id)
                {
                    return index;
                }
            }

            return -1;
        }
    }

    [Serializable]
    public struct FireworkSizeDefinition
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private float _launchHeight;
        [SerializeField] private float _burstDiameter;
        [SerializeField] private float _visualScale;

        public float VisualScale => _visualScale;
        public float LaunchHeight => _launchHeight;
        public float BurstDiameter => _burstDiameter;
        public string Id => _id;
        public string DisplayName => _displayName;
    }

    [Serializable]
    public struct FireworkColorDefinition
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private Color _value;

        public Color Value => _value;
        public string Id => _id;
        public string DisplayName => _displayName;
    }

    public readonly struct FireworkSelection
    {
        public FireworkSelection(
            FireworkSizeDefinition size,
            FireworkTypeDefinition effect,
            Color color,
            Color changeColor)
        {
            Size = size;
            Effect = effect;
            Color = color;
            ChangeColor = changeColor;
        }

        public FireworkSizeDefinition Size { get; }
        public FireworkTypeDefinition Effect { get; }
        public Color Color { get; }
        public Color ChangeColor { get; }
    }
}
