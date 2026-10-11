using UnityEngine;
using UnityEngine.UI;

namespace Fireworks
{
    /// <summary>フォントの記号収録状況に依存せず、小さな歯車を描画する。</summary>
    public sealed class SettingsIconGraphic : MaskableGraphic
    {
        private const int SEGMENT_COUNT = 64;
        private const int SEGMENTS_PER_TOOTH = 8;
        private const int TOOTH_START = 2;
        private const int TOOTH_END = 5;
        private const float ROOT_RADIUS_RATIO = 0.78f;
        private const float HOLE_RADIUS_RATIO = 0.42f;
        private const float PROGRESS_INNER_RADIUS_RATIO = 1.15f;
        private const float PROGRESS_OUTER_RADIUS_RATIO = 1.3f;
        private float _holdProgress;

        public float HoldProgress
        {
            set
            {
                _holdProgress = Mathf.Clamp01(value);
                SetVerticesDirty();
            }
        }

        protected override void OnPopulateMesh(VertexHelper vertices)
        {
            vertices.Clear();
            Rect rect = GetPixelAdjustedRect();
            float radius = Mathf.Min(rect.width, rect.height) * 0.5f;
            for (int segment = 0; segment < SEGMENT_COUNT; segment++)
            {
                int next = (segment + 1) % SEGMENT_COUNT;
                Vector2 direction = GetDirection(segment);
                Vector2 nextDirection = GetDirection(next);
                int first = vertices.currentVertCount;
                vertices.AddVert(rect.center + direction * radius * GetOuterRadius(segment), color, Vector2.zero);
                vertices.AddVert(rect.center + direction * radius * HOLE_RADIUS_RATIO, color, Vector2.zero);
                vertices.AddVert(rect.center + nextDirection * radius * HOLE_RADIUS_RATIO, color, Vector2.zero);
                vertices.AddVert(rect.center + nextDirection * radius * GetOuterRadius(next), color, Vector2.zero);
                vertices.AddTriangle(first, first + 1, first + 2);
                vertices.AddTriangle(first + 2, first + 3, first);
            }

            // 歯車を隠さず、外周の円弧で長押しの進み具合を伝える。
            float progressSegments = _holdProgress * SEGMENT_COUNT;
            for (int segment = 0; segment < Mathf.CeilToInt(progressSegments); segment++)
            {
                float startAngle = Mathf.PI * 0.5f - segment * Mathf.PI * 2f / SEGMENT_COUNT;
                float endAngle = Mathf.PI * 0.5f - Mathf.Min(segment + 1f, progressSegments) * Mathf.PI * 2f / SEGMENT_COUNT;
                Vector2 start = new Vector2(Mathf.Cos(startAngle), Mathf.Sin(startAngle));
                Vector2 end = new Vector2(Mathf.Cos(endAngle), Mathf.Sin(endAngle));
                int first = vertices.currentVertCount;
                vertices.AddVert(rect.center + start * radius * PROGRESS_OUTER_RADIUS_RATIO, Color.white, Vector2.zero);
                vertices.AddVert(rect.center + start * radius * PROGRESS_INNER_RADIUS_RATIO, Color.white, Vector2.zero);
                vertices.AddVert(rect.center + end * radius * PROGRESS_INNER_RADIUS_RATIO, Color.white, Vector2.zero);
                vertices.AddVert(rect.center + end * radius * PROGRESS_OUTER_RADIUS_RATIO, Color.white, Vector2.zero);
                vertices.AddTriangle(first, first + 1, first + 2);
                vertices.AddTriangle(first + 2, first + 3, first);
            }
        }

        private static Vector2 GetDirection(int segment)
        {
            float angle = segment * Mathf.PI * 2f / SEGMENT_COUNT;
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }

        private static float GetOuterRadius(int segment)
        {
            int toothSegment = segment % SEGMENTS_PER_TOOTH;
            return toothSegment >= TOOTH_START && toothSegment <= TOOTH_END ? 1f : ROOT_RADIUS_RATIO;
        }
    }
}
