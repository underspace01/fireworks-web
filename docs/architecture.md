# Architecture

## Engine

Unity 6 LTS

## Render Pipeline

URP

## Input

Unity Input System

## Effect

Particle System

## Network

HTTP

## Data

ScriptableObject

## UI

uGUI

日本語テキストはTextMeshProとNoto Sans CJK JPを使用する。

待機中のLAN操作用QRコードは、`LanAccessQrDisplay` がZXing.Netで生成し、uGUIの `RawImage` として右下に表示する。

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

文字花火とPCのuGUIで共用するNoto Sans CJK JP Font Assetは `Assets/Art/Fonts` に置く。`TextFirework.prefab` とMainSceneの `FireworkDebugUI` がInspector参照し、このフォントの読込みにResourcesは使用しない。

ZXing.Netの通知とApache License 2.0本文、Noto Sans CJK JPのOFL 1.1本文は、`StreamingAssets/ThirdPartyLicenses.txt` としてビルドに含める。`ThirdPartyLicensePanel` がこのファイルをデバッグUIのスクロールパネルに表示する。フォント付属の `OFL.txt` も原本として保持する。

実行時のPC UIは責務ごとに分割する。`FireworkDebugUI` はCanvas・キー入力・カーソル・各パネルの表示状態を調停し、`FireworkParameterPanel` は花火パラメーターと打上操作、`ApplicationInfoPanel` はバージョン表示と終了操作、`ThirdPartyLicensePanel` はライセンス本文の読込みと表示を担当する。共通のuGUI生成処理は `RuntimeUiFactory` に集約する。

Addressablesは使用しない。

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
