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
