# Architecture

## Engine

Unity 6 LTS

## Render Pipeline

URP

## Input

Unity Input System

## Effect

Particle System

## Platform

Unity Web（WebGL）。操作から描画まで端末内で実行し、LAN通信による遠隔操作は行わない。HTTP/HTTPSはビルドと同梱ファイルの配信にのみ使用する。

## Data

ScriptableObject

## UI

uGUI

日本語テキストはTextMeshProとNoto Sans CJK JPを使用する。

タップとクリックはuGUIの全面ボタンから受け付ける。小さな歯車アイコンだけを常時表示し、情報画面は設定パネル内から開く。パネル表示中は画面タップによる打上を停止する。CanvasScalerは短辺480を基準にExpandで拡縮する。

## Scene

初期は1Sceneのみ

MainScene

## 名前空間

Fireworks

## フォルダ構成

Assets

Scripts

Prefabs

Particles

ScriptableObjects

Audio

UI

StreamingAssets

文字花火とuGUIで共用するNoto Sans CJK JP Font Assetは `Assets/Art/Fonts` に置く。`TextFirework.prefab` とMainSceneの `FireworkDebugUI` がInspector参照し、このフォントの読込みにResourcesは使用しない。

Noto Sans CJK JPのOFL 1.1本文は、`StreamingAssets/ThirdPartyLicenses.txt` としてビルドに含める。`ThirdPartyLicensePanel` がこのファイルを情報UIのスクロールパネルに表示する。フォント付属の `OFL.txt` も原本として保持する。

実行時のUIは責務ごとに分割する。`FireworkDebugUI` はCanvas・キー入力・タップ入力・各パネルの表示状態を調停し、`FireworkParameterPanel` は花火パラメーターと打上操作、`ApplicationInfoPanel` はバージョン表示（Webでは終了ボタンなし）、`ThirdPartyLicensePanel` はライセンス本文の読込みと表示を担当する。共通のuGUI生成処理は `RuntimeUiFactory` に集約する。

Addressablesは使用しない。
歯車の入力は `SettingsHoldButton` がuGUIイベントと押下中だけ動くCoroutineで短いタップ・長押しを分離する。`SettingsIconGraphic` が長押しの進捗円弧を描画する。

---

# データ管理

花火仕様は FIRE_PARAMETER.md を参照する。

ScriptableObjectで管理する。

ゲーム内へハードコードしない。

FireworkSettingsは、花火のサイズ・種類Prefab・色、打上高度の揺らぎ、画面上端・下端の高度を管理する。種類Prefabは `FireworkTypeDefinition` を持ち、通常花火は `FireworkEffect`、文字花火は `TextFireworkEffect` を管理する。文字花火の字形から粒子配置を作る処理は `TextFireworkGlyphSampler` に分離する。

---

# 開発方針

小さく実装する。

まず動くものを作る。

その後演出を追加する。

## Webビルド

`FireworkWebBuild` のEditorメニューでWeb設定を適用し、`Builds/Web`へビルドする。`Assets/WebGLTemplates/Fireworks`で画面回転とセーフエリアに対応し、描画ピクセル比は最大2とする。Gzipと展開フォールバックを使用する。ライセンス本文はUnityWebRequestで非同期に取得する。
