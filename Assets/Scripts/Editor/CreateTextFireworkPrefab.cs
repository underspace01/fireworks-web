using UnityEditor;
using TMPro;
using UnityEngine;

namespace Fireworks.Editor
{
    /// <summary>文字花火を種類別Prefabとして生成し、設定アセットへ登録する。</summary>
    public static class CreateTextFireworkPrefab
    {
        private const string PREFAB_PATH = "Assets/Prefabs/Types/TextFirework.prefab";
        private const string SETTINGS_PATH = "Assets/ScriptableObjects/FireworkSettings.asset";
        private const string FONT_ASSET_PATH = "Assets/Art/Fonts/NotoSansCJKjp-Regular SDF.asset";

        [MenuItem("Tools/Fireworks/文字花火Prefabを生成")]
        public static void Create()
        {
            GameObject root = new GameObject("TextFirework");
            TextFireworkEffect effect = root.AddComponent<TextFireworkEffect>();
            FireworkTypeDefinition definition = root.AddComponent<FireworkTypeDefinition>();
            ParticleSystem launchParticles = CreateParticleSystem(root.transform, "LaunchParticles");
            ParticleSystem textParticles = CreateParticleSystem(root.transform, "TextParticles");
            TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_ASSET_PATH);
            if (fontAsset == null)
            {
                Debug.LogError("文字花火用のTMP Font Assetが見つかりません。");
                Object.DestroyImmediate(root);
                return;
            }

            effect.Configure(launchParticles, textParticles, fontAsset);
            definition.ConfigureTextFirework(effect);

            PrefabUtility.SaveAsPrefabAsset(root, PREFAB_PATH);
            Object.DestroyImmediate(root);

            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);
            FireworkTypeDefinition prefab = prefabRoot == null ? null : prefabRoot.GetComponent<FireworkTypeDefinition>();
            FireworkSettings settings = AssetDatabase.LoadAssetAtPath<FireworkSettings>(SETTINGS_PATH);
            if (prefab == null || settings == null)
            {
                Debug.LogError("文字花火PrefabまたはFireworkSettingsを取得できません。");
                return;
            }

            settings.RegisterEffect(prefab);
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static ParticleSystem CreateParticleSystem(Transform parent, string name)
        {
            GameObject particleObject = new GameObject(name);
            particleObject.transform.SetParent(parent, false);
            ParticleSystem particleSystem = particleObject.AddComponent<ParticleSystem>();
            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystem.MainModule main = particleSystem.main;
            main.playOnAwake = false;
            ParticleSystem.EmissionModule emission = particleSystem.emission;
            emission.enabled = false;
            return particleSystem;
        }
    }
}
