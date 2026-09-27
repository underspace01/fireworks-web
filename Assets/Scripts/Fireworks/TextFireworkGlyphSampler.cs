using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Fireworks
{
    /// <summary>TextMeshProの字形から、文字花火用の粒子配置を作る。</summary>
    public static class TextFireworkGlyphSampler
    {
        private const int SAMPLES_PER_CHARACTER_SIDE = 12;
        private const int MAXIMUM_PARTICLE_COUNT = 800;
        private const float GLYPH_ALPHA_THRESHOLD = 0.5f;
        private const float MAXIMUM_VIEWPORT_FRACTION = 0.75f;

        public static List<Vector3> CreatePositions(
            Transform parent, string value, TMP_FontAsset fontAsset, Camera targetCamera, float visualScale)
        {
            fontAsset.TryAddCharacters(value, out _);
            GameObject layoutObject = new GameObject("TextFireworkLayout");
            layoutObject.transform.SetParent(parent, false);
            TextMeshPro textMesh = layoutObject.AddComponent<TextMeshPro>();
            textMesh.font = fontAsset;
            textMesh.text = value;
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.textWrappingMode = TextWrappingModes.NoWrap;
            textMesh.overflowMode = TextOverflowModes.Overflow;
            textMesh.fontSize = 5f * visualScale;
            textMesh.ForceMeshUpdate();

            float maximumWidth = targetCamera.orthographicSize * 2f *
                MAXIMUM_VIEWPORT_FRACTION;
            float textScale = textMesh.preferredWidth > maximumWidth && textMesh.preferredWidth > 0f
                ? maximumWidth / textMesh.preferredWidth : 1f;
            List<Vector3> positions = CollectPositions(textMesh.textInfo, fontAsset.atlasTexture, textScale);
            // 収まる文字列は位置を補正し、画面幅を超える場合は縮小せず中央で見切れを許容する。
            if (positions.Count > 0)
            {
                float minimumX = float.PositiveInfinity;
                float maximumX = float.NegativeInfinity;
                foreach (Vector3 position in positions)
                {
                    minimumX = Mathf.Min(minimumX, position.x);
                    maximumX = Mathf.Max(maximumX, position.x);
                }
                float halfWidth = targetCamera.orthographicSize * targetCamera.aspect;
                float centerX = targetCamera.transform.position.x - parent.position.x;
                float offset = maximumX - minimumX > halfWidth * 2f
                    ? centerX - (minimumX + maximumX) * 0.5f
                    : Mathf.Clamp(0f, centerX - halfWidth - minimumX, centerX + halfWidth - maximumX);
                for (int index = 0; index < positions.Count; index++)
                    positions[index] += Vector3.right * offset;
            }
            Object.Destroy(layoutObject);
            return positions;
        }

        private static List<Vector3> CollectPositions(TMP_TextInfo textInfo, Texture2D atlas, float scale)
        {
            List<Vector3> positions = new List<Vector3>();
            if (atlas == null || !atlas.isReadable) return positions;
            for (int i = 0; i < textInfo.characterCount && positions.Count < MAXIMUM_PARTICLE_COUNT; i++)
            {
                TMP_CharacterInfo character = textInfo.characterInfo[i];
                if (!character.isVisible) continue;
                for (int y = 0; y < SAMPLES_PER_CHARACTER_SIDE && positions.Count < MAXIMUM_PARTICLE_COUNT; y++)
                for (int x = 0; x < SAMPLES_PER_CHARACTER_SIDE && positions.Count < MAXIMUM_PARTICLE_COUNT; x++)
                {
                    float horizontal = (x + 0.5f) / SAMPLES_PER_CHARACTER_SIDE;
                    float vertical = (y + 0.5f) / SAMPLES_PER_CHARACTER_SIDE;
                    Vector2 uv = Interpolate((Vector2)character.vertex_BL.uv, (Vector2)character.vertex_BR.uv,
                        (Vector2)character.vertex_TL.uv, (Vector2)character.vertex_TR.uv, horizontal, vertical);
                    if (atlas.GetPixelBilinear(uv.x, uv.y).a < GLYPH_ALPHA_THRESHOLD) continue;
                    positions.Add(Interpolate(character.vertex_BL.position, character.vertex_BR.position,
                        character.vertex_TL.position, character.vertex_TR.position, horizontal, vertical) * scale);
                }
            }
            return positions;
        }

        private static Vector2 Interpolate(Vector2 bl, Vector2 br, Vector2 tl, Vector2 tr, float x, float y) =>
            Vector2.Lerp(Vector2.Lerp(bl, br, x), Vector2.Lerp(tl, tr, x), y);

        private static Vector3 Interpolate(Vector3 bl, Vector3 br, Vector3 tl, Vector3 tr, float x, float y) =>
            Vector3.Lerp(Vector3.Lerp(bl, br, x), Vector3.Lerp(tl, tr, x), y);
    }
}
