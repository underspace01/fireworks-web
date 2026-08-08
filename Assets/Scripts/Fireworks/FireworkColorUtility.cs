using UnityEngine;

namespace Fireworks
{
    /// <summary>Bloom時の色相ごとの明るさ差を揃える処理をまとめる。</summary>
    public static class FireworkColorUtility
    {
        public static Color NormalizeIntensity(Color color)
        {
            float magnitude = Mathf.Sqrt((color.r * color.r) + (color.g * color.g) + (color.b * color.b));
            return magnitude <= 0f
                ? Color.black
                : new Color(color.r / magnitude, color.g / magnitude, color.b / magnitude, color.a);
        }
    }
}
