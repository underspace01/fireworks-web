using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace Fireworks.Editor
{
    /// <summary>文字花火用の動的Noto Sans CJK JPフォントアセットを生成する。</summary>
    public static class CreateTextFireworkFontAsset
    {
        private const string SOURCE_FONT_PATH = "Assets/Art/Fonts/NotoSansCJKjp-Regular.otf";
        private const string OUTPUT_PATH = "Assets/Art/Fonts/NotoSansCJKjp-Regular SDF.asset";
        private const string BASIC_CHARACTERS =
            "あいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほまみむめもやゆよらりるれろわをん" +
            "がぎぐげござじずぜぞだぢづでどばびぶべぼぱぴぷぺぽぁぃぅぇぉゃゅょっゔ" +
            "アイウエオカキクケコサシスセソタチツテトナニヌネノハヒフヘホマミムメモヤユヨラリルレロワヲン" +
            "ガギグゲゴザジズゼゾダヂヅデドバビブベボパピプペポァィゥェォャュョッヴ" +
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 !?ー・";

        [MenuItem("Tools/Fireworks/文字花火用フォントを生成")]
        public static void Create()
        {
            Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(SOURCE_FONT_PATH);
            if (sourceFont == null)
            {
                Debug.LogError("Noto Sans CJK JPフォントが見つかりません。");
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(OUTPUT_PATH) != null)
            {
                // 以前の生成ではアトラスをサブアセットとして保存していなかったため作り直す。
                AssetDatabase.DeleteAsset(OUTPUT_PATH);
            }

            TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
                sourceFont,
                90,
                9,
                GlyphRenderMode.SDFAA,
                2048,
                2048,
                AtlasPopulationMode.Dynamic,
                true);
            if (fontAsset == null)
            {
                Debug.LogError("文字花火用のTMP Font Assetを生成できませんでした。");
                return;
            }

            AssetDatabase.CreateAsset(fontAsset, OUTPUT_PATH);
            foreach (Texture2D atlasTexture in fontAsset.atlasTextures)
            {
                if (atlasTexture != null)
                {
                    AssetDatabase.AddObjectToAsset(atlasTexture, fontAsset);
                }
            }

            if (fontAsset.material != null)
            {
                AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
            }

            // よく使う文字は編集時にアトラスへ焼き込み、実行時の字形生成に依存しない。
            fontAsset.TryAddCharacters(BASIC_CHARACTERS, out _);
            EditorUtility.SetDirty(fontAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
