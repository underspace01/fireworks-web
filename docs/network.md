# LAN操作機能設計

## 目的

同一LAN内のスマートフォン・タブレットのブラウザから、PC上で実行中のUnityアプリを操作する。

アプリは外部公開しない。ルーターのポート転送は設定しない。

## 対象プラットフォーム

UnityアプリはPC向けスタンドアロン版を対象とする。スマートフォン・タブレットはブラウザのみを使用し、アプリのインストールは不要とする。

## 構成

```text
スマートフォン / タブレット
  Browser
    ├─ HTTP GET  : 操作画面と設定情報を取得
    └─ HTTP POST : 打上コマンドを送信
             │
             ▼
PCのUnityアプリ
  LanWebServer ── FireworkCommandQueue ── WebFireworkController ── FireworkLauncher
```

`LanWebServer` はネットワーク受信だけを担当する。Unity APIには触れず、検証済みコマンドを `FireworkCommandQueue` へ追加する。

`WebFireworkController` は `Update` でキューを処理し、Unityのメインスレッドから `FireworkLauncher` を呼び出す。

複数端末からの接続とコマンドを受け付ける。全端末のコマンドは1つのスレッドセーフなキューへ追加し、到着順にメインスレッドで実行する。

## HTTPサーバー

|項目|内容|
|---|---|
|待受アドレス|全LANインターフェース|
|既定ポート|8080（Inspectorで変更可能）|
|操作画面|`Assets/StreamingAssets/WebController/index.html`|
|設定取得|`GET /api/config`|
|状態取得|`GET /api/status`|

操作画面はHTML、CSS、JavaScriptの静的ファイルとして配信する。`StreamingAssets` を使用するため、PC向けビルドにもそのまま含まれる。

`GET /api/config` は、選択可能なサイズ・花火種類・色と接続用トークンを返す。花火種類は `FireworkSettings` に登録された種類Prefabを返し、各種類には色変化を許可するか、文字花火かどうかを含める。

サイズには波紋表示用の視覚スケール、色には波紋表示用のRGB値も含める。

`POST /api/launch` は `type: "launch"` のUTF-8 JSONだけを受け付ける。文字花火も `structure` に文字花火のIDを指定する通常の打上コマンドとして扱う。文字花火では `textValue` を必須とし、前後の空白を除去した制御文字を含まない1〜6文字に制限する。

ランダム選択はブラウザ側で行う。ブラウザは設定情報に含まれる全種類・全サイズ・全色から等確率で選び、選択結果を `launch` コマンドとして送信する。

## 打上コマンドAPI

打上コマンドは HTTP POST で送信する。

```text
POST http://<PCのLAN IP>:8080/api/launch?token=<接続トークン>
```

本文はJSONとする。

### クライアントからのコマンド

```json
{
  "type": "launch",
  "requestId": "uuid",
  "sizeId": "SZ08",
  "structure": "Chrysanthemum",
  "colorId": "CL05",
  "changeColorId": "CL03"
}
```

色変化を許可しないエフェクトでは `changeColorId` を無視する。存在しないID・エフェクト、または不正なJSONは実行せず、エラーを返す。

文字花火を指定する例は以下とする。

```json
{
  "type": "launch",
  "requestId": "uuid",
  "sizeId": "SZ08",
  "structure": "Text",
  "colorId": "CL05",
  "textValue": "花火大会"
}
```

### HTTP応答

```json
{ "type": "accepted", "requestId": "uuid" }
```

```json
{ "type": "error", "requestId": "uuid", "message": "不正なエフェクトです" }
```

`POST /api/launch` の応答で受理またはエラーを返す。持続接続は使用しない。

## セキュリティ

- サーバーはLAN内でのみ利用し、外部公開しない。
- 起動時にランダムな接続トークンを生成する。
- `POST /api/launch` のトークンを検証する。
- LANのプライベートIPv4アドレス以外からの接続は拒否する。
- 静的ファイルの要求は `WebController` 配下に限定し、パストラバーサルを拒否する。
- 1メッセージの上限を設定し、接続数と端末ごとの打上要求頻度を制限する。

PCのファイアウォールでは、設定したポート（既定8080）のローカルネットワークからの受信を許可する必要がある。

## UI

操作画面は uGUIではなくブラウザ用のHTMLで作成する。初期表示ではパラメーター変更UIを隠し、画面の隅にある設定ボタンだけを常時表示する。

- 設定UIが閉じているとき: 画面の設定ボタン以外をタップすると、ブラウザが選んだランダムな花火を打ち上げる。
- 設定UIが開いているとき: サイズ・エフェクト・基本色・変化色（エフェクトが許可する場合だけ表示）を選び、打上ボタンで指定した花火を打ち上げる。文字花火を選んだ場合だけ文字入力欄を表示し、変化色は表示しない。
- 設定ボタンは設定UIの表示・非表示を切り替える。設定UIと設定ボタンへのタップは、ランダム打上として扱わない。
- 接続状態・エラー表示は常時表示する。

各端末は独立して設定値を保持する。ある端末での設定変更は、他の端末の設定画面へ影響しない。

PC画面の右下には、LAN操作画面の接続URLを埋め込んだQRコードを表示する。花火が1つでも再生中の場合はQRコードを隠し、すべての花火が完全に再生し終わってから3秒後に再表示する。QRコードは1マスを整数ピクセルで描画し、テクスチャ座標の上下反転を補正してカメラで読み取りやすくする。再表示までの時間と表示サイズ・余白は `LanAccessQrDisplay` のInspectorで調整できる。

## 利用ライブラリ

HTTPとJSONはUnity標準機能を使用する。

- HTTP: `System.Net.HttpListener`
- JSON: Unity標準の `JsonUtility`
- QRコード生成: ZXing.Net 0.16.11のUnity向け公式アセンブリ（Apache-2.0）

ZXing.Netは公式リリースのDLLのみを `Assets/Plugins` に配置する。第三者ソースはリポジトリへ含めず、ライセンス情報とアセンブリのSHA-256は `ThirdPartyNotices.md` に記録する。

## 実装順序

1. `LanWebServer` でHTTPのヘルスチェックと静的HTML配信を実装する。
2. HTTP POSTのトークン検証とJSON受信を実装する。
3. メインスレッド用コマンドキューと `WebFireworkController` を実装する。
4. Web操作画面、URL表示、QRコード表示を実装する。
5. LAN実機テストとファイアウォール設定手順を確認する。

## テスト項目

- PC自身と同一Wi-Fiのスマートフォンから操作画面を表示できる。
- トークンなし・不正トークンでは操作できない。
- 未登録のエフェクトIDを送っても打ち上がらない。
- 有効なコマンドで指定した花火が1回だけ打ち上がる。
- 複数端末から同時に操作してもUnity側で例外が発生しない。
- 複数端末が同時にランダム打上を行った場合、全てのコマンドが到着順に実行される。
- 切断後にブラウザが再接続できる。
- 待機中はPC画面右下のQRコードを読み取り、操作画面を開ける。
- 花火の打上開始時にQRコードが消え、すべての花火の再生終了から3秒後に再表示される。
- ポートが使用中の場合に、PC画面へ分かりやすくエラーを表示する。
