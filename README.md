# Fireworks

スマートフォン・タブレットのブラウザだけで操作と表示が完結する、Unity製の花火シミュレーションです。縦横両向きに対応し、PCブラウザでも楽しめます。端末ごとに独立して動作します。

Codexの使い方を勉強するために作ったプロジェクトで、ほぼCodexのみで作っています。

[fireworks](https://github.com/underspace01/fireworks)をブラウザのみで使えるようにしたものです。

## 主な機能

- 牡丹、菊、変化菊、冠、文字花火
- 玉のサイズに応じた開花直径、粒子数、打上高度、効果音
- Bloomを使った発光表現と、打上音・破裂音
- 最大6文字を発光粒子で描く文字花火
- 画面タップによるランダム打上と、設定パネルからの指定打上
- 小さな歯車アイコンから開く設定パネルとアプリ情報

画面サイズや向きを変えても、画面高に対する打上高度・開花直径の比率を維持します。縦長画面では左右の見切れを許容します。

## 動作環境

- 開発：Unity 6000.5.6f1、Web Build Support、Universal Render Pipeline（URP）
- 実行：スマートフォン・タブレット・PCのWebブラウザ（HTTP/HTTPS配信）

MacのChrome・Safariで音声再生を確認済みです。スマートフォン実機での検証状況は [Web配信](docs/network.md) を参照してください。

## 操作方法

読み込み後に「はじめる」を押します。

| 操作 | 内容 |
|---|---|
| 画面をタップ／クリック | 設定・情報パネルを閉じている間、ランダムな花火を打ち上げる |
| 右下の歯車 | 設定パネルを表示／非表示 |
| 設定パネルの打上ボタン | 指定した花火、またはランダムな花火を打ち上げる |
| 設定パネルの「アプリ情報」 | バージョン・サードパーティーライセンスを表示 |
| Space（PC） | パネルを閉じている間、ランダムな花火を打ち上げる |
| Slash（PC） | 設定パネルを表示／非表示 |
| Escape（PC） | アプリ情報を表示／非表示。ライセンス表示中は情報パネルへ戻る |

設定ではサイズ・構造・基本色を指定でき、変化菊では変化色も選択できます。文字花火を選ぶと最大6文字の入力欄が表示されます。ランダム打上で文字花火が選ばれた場合は入力文字列を使い、空ならランダムな英大文字1文字を表示します。

設定内容が画面に収まらない場合はパネル内をスクロールできます。情報パネルを閉じると設定パネルに戻ります。

## 開発・Webビルド

1. Unity HubでUnity 6000.5.6f1とWeb Build Supportをインストールします。
2. このプロジェクトを開き、`Assets/Scenes/MainScene.unity` を開きます。
3. Editorで確認する場合はPlayモードで実行します。
4. `Tools > Fireworks > Webビルド` を実行します。Web向け設定が適用され、`Builds/Web` に出力されます。

`Builds/Web` の `index.html`、`Build/`、`StreamingAssets/` をまとめて静的Webホスティングへ配置します。公開後は開発用PCを起動しておく必要はありません。スマホはインターネット経由で公開URLを開いて利用します。

`index.html` のダブルクリック（`file://`）では起動できません。`Assets/WebGLTemplates` 内のHTMLはビルド用の雛形です。開発時のローカル確認方法は [Web配信](docs/network.md#ローカル確認) を参照してください。

## GitHub Pagesでの公開

ソースコードとWeb版を同じ公開リポジトリで管理する場合は、次の構成で公開できます。

| ブランチ | 内容 |
|---|---|
| `main` | Unityプロジェクトと仕様書 |
| `gh-pages` | `Builds/Web` の中身と空の `.nojekyll` ファイル |

1. `gh-pages` ブランチの直下に `index.html`、`Build/`、`StreamingAssets/`、`.nojekyll` を配置してpushします。
2. GitHubの `Settings > Pages` で `Deploy from a branch` を選択します。
3. ブランチを `gh-pages`、フォルダを `/ (root)` にして保存します。
4. Pages設定画面に表示される公開URLをスマホで開きます。

公開設定後のURLは `https://underspace01.github.io/fireworks-web/` です。この手順の記載は公開完了を示すものではありません。

更新時はUnityで再ビルドして、出力を `gh-pages` へ反映します。`main` へのソースコードのpushだけではWeb版は更新されません。`Builds/` はGit管理対象外です。

詳細は [GitHub Pagesの公開元設定](https://docs.github.com/en/pages/getting-started-with-github-pages/configuring-a-publishing-source-for-your-github-pages-site) を参照してください。

## プロジェクト構成

- `Assets/Scenes/MainScene.unity`：メインシーン
- `Assets/Prefabs/Types`：花火種類ごとのPrefab
- `Assets/ScriptableObjects/FireworkSettings.asset`：サイズ・種類・色などの設定
- `Assets/Scripts/Fireworks`：花火の打上・描画・音声処理
- `Assets/Scripts/UI`：ブラウザ内で操作するuGUI
- `Assets/Scripts/Editor/FireworkWebBuild.cs`：Web設定・ビルドメニュー
- `Assets/WebGLTemplates/Fireworks`：Web版の起動画面
- `Assets/StreamingAssets/ThirdPartyLicenses.txt`：実行時に表示するライセンス本文
- `docs`：仕様・設計資料

新しい花火は既存の種類Prefabを複製し、`FireworkTypeDefinition` を設定して `FireworkSettings` の `Effects` に追加します。詳細は [花火仕様](docs/fireworks.md) を参照してください。

## ドキュメント

- [プロダクト方針](docs/vision.md)
- [花火仕様](docs/fireworks.md)
- [UI仕様](docs/ui.md)
- [Web配信](docs/network.md)
- [アーキテクチャ](docs/architecture.md)

## サードパーティーライセンス

[第三者ライセンス一覧](ThirdPartyNotices.md) を参照してください。

## 効果音の追加

MainSceneの `Firework System` にある `FireworkAudio` コンポーネントの `Launch Clip` と `Explosion Clip` へ、打上音と破裂音をそれぞれ割り当ててください。リポジトリにはサンプル音源を含み、追加したローカル音源はGit管理対象外です。詳細は [花火仕様の効果音](docs/fireworks.md#効果音) を参照してください。
