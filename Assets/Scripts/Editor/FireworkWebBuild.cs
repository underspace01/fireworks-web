using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Fireworks.Editor
{
    /// <summary>開発環境とCIで同じWeb設定を再現する。</summary>
    internal static class FireworkWebBuild
    {
        private const string OUTPUT_PATH = "Builds/Web";
        private const string MAIN_SCENE = "Assets/Scenes/MainScene.unity";
        private const int INITIAL_MEMORY_MB = 128;

        [MenuItem("Tools/Fireworks/Web設定を適用")]
        private static void ApplySettings()
        {
            var target = NamedBuildTarget.WebGL;
            PlayerSettings.WebGL.template = "PROJECT:Fireworks";
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            // 配信先が未定のため、圧縮ヘッダーを設定できない静的ホストでも読み込めるようにする。
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.WebGL.initialMemorySize = INITIAL_MEMORY_MB;
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.SetManagedStrippingLevel(target, ManagedStrippingLevel.Medium);
            PlayerSettings.SetIl2CppCodeGeneration(target, Il2CppCodeGeneration.OptimizeSize);
            PlayerSettings.SetApiCompatibilityLevel(target, ApiCompatibilityLevel.NET_Standard_2_0);
            PlayerSettings.runInBackground = false;
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Tools/Fireworks/Webビルド")]
        private static void Build()
        {
            ApplySettings();
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { MAIN_SCENE },
                locationPathName = OUTPUT_PATH,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException("Webビルドに失敗しました: " + report.summary.result);
            }
        }
    }
}
