# AGENTS.md

# Project

このプロジェクトは Unity 6 を使用した花火シミュレーションアプリです。

目的は、リアルな花火を自由に配置・編集・再生できるアプリを作ることです。

# Specification

- アプリ仕様は、./docs/*.md に記載していく。
- AIにて変更不可なもの
  - FIRE_PARAMETER.md
- AIにて変更不可なものも含めて、人の手で変更されたものは適切に反映させること。ただし他の部分の仕様と辻褄が合わない場合は、実装の前に必ず確認を取ること。

---

# AI Guidelines

コードを生成・編集する際は以下を守ってください。

- 必要最小限の変更のみ行う
- 関係ないファイルは編集しない
- 既存の設計を尊重する
- 既存コメントは削除しない
- コンパイルエラーが発生しないこと
- 日本語コメントを書く
- 過剰な設計をしない
- 不要なライブラリを追加しない

---

# Unity Version

Unity 6

Rendering

- Universal Render Pipeline (URP)

Language

- C#

IDE

- Visual Studio Code

---

# Coding Style

## Access Modifier

基本は private

Inspectorに表示する場合のみ

```csharp
[SerializeField]
private XXX value;
```

public フィールドは禁止。

---

## Naming

Class

PascalCase

```
FireworkLauncher
```

Method

PascalCase

```
LaunchFirework()
```

Private Field

```
_spawnPoint
```

Local Variable

```
firework
```

定数

```
MAX_COUNT
```

---

## Inspector

可能な限り Inspector から調整できるようにする。

マジックナンバーは禁止。

---

## UI

- 実行時に表示するUIは、原則として uGUI を使用する。
- 日本語を表示するUIテキストには、TextMeshPro と Noto Sans CJK JP を使用する。
- IMGUI（OnGUI）は、実行時UIには使用しない。

---

## Update

Update は必要最小限。

毎フレーム処理が不要なら

- Coroutine
- Event
- Invoke

を優先する。

---

## Null Check

NullReferenceException を起こさないようにする。

適切に null チェックを行う。

---

## Logging

Debug.Log はデバッグ用途のみ。

最終コードには不要なログを残さない。

---

# Folder Structure

```
Assets

├── Art
├── Audio
├── Materials
├── Prefabs
├── Resources
├── Scenes
├── ScriptableObjects
├── Scripts
│   ├── Core
│   ├── Fireworks
│   ├── UI
│   └── Utility
├── Settings
└── VFX
```

新規コードは Scripts 以下に作成する。

Resourcesフォルダは使用しない。Resourcesの動的ロードは使用しない。

---

# Architecture

基本方針

- 単一責任を意識する
- クラスは小さく保つ
- ScriptableObject を活用する
- 依存関係を減らす

巨大な Manager を作らない。

---

# Performance

GC Alloc を減らす。

毎フレーム

- Find()
- GetComponent()
- LINQ

を繰り返さない。

---

# Comments

コメントは日本語。

なぜその処理をするのかを書く。

コードをそのまま説明するコメントは不要。

---

# Git

不要なファイルは作らない。

meta ファイルは Unity の管理に従う。

コミット前に、変更内容が docs/*.md の仕様・設計へ反映されているか確認する。

仕様または設計に影響する変更は、関連するドキュメント更新を同じコミットに含める。

---

# Task Policy

大きな機能を実装するときは

1. 設計
2. 実装
3. テスト方法

の順で提案する。

一度に巨大な変更を行わず、小さな単位で実装する。

---

# When Unsure

仕様が曖昧なら推測で実装しない。

必ず確認事項を提示する。

---

# Goal

読みやすく保守しやすいコードを最優先とす
