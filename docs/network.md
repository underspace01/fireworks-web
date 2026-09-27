# Web配信

## 方針

旧LAN遠隔操作機能は廃止する。PCアプリ、HTTP待受サーバー、接続トークン、打上API、コマンドキュー、QRコード、旧HTML操作ページは使用しない。
スマートフォン・タブレットのブラウザ内でUnity Webを実行し、端末ごとに独立して操作・描画する。端末間の同期は行わない。

## ビルドと配信

Unity 6000.5.6f1とWeb Build Supportを使用する。
Tools > Fireworks > Web設定を適用で設定を再現できる。
Tools > Fireworks > WebビルドからBuilds/Webへ出力する。
出力フォルダ全体（index.html、Build、StreamingAssets）を静的Webサーバーへ配置する。
ローカルファイルを直接開くfile://では使用しない。公開時はHTTPS配信とする。

Gzip圧縮とDecompression Fallbackを有効にし、圧縮ヘッダーを設定できないホストでも読み込める構成とする。
初期メモリ128MB、Managed Stripping Medium、IL2CPP OptimizeSizeを使用する。
FIRE_PARAMETER.mdの物理パラメーターは変更しない。

## 検証

- Webビルドが成功する。
- 静的HTTP/HTTPS配信からデータ、Wasm、ライセンス本文を取得できる。
- LANサーバーや旧/api/*への通信が発生しない。
- スマホのタップ、画面回転、文字入力、音声再生を実機で確認する（詳細はui.md）。
- 複数端末で開いても各端末内だけに花火が表示される。

## 移行時の検証状況

- Web向けおよびEditor向けC#のコンパイル確認済み。
- WebGLターゲットへの切替とWeb設定の保存を確認済み。
- 最新コードのWebビルド成功（エラー0件）。出力からHTTP配信で起動を確認済み。
- Chromeのスマホ画面エミュレーションで起動・打上・設定パネル・縦横表示を確認済み。
- WebKit検証環境で打上音・破裂音の読み込みと再生開始を確認済み。MacのChromeとSafariで音が聞こえることをユーザーが確認済み（Safariの一時的な無音はブラウザ再起動で解消）。
- iOS Safari・Android Chromeの実機での動作確認は未実施。

Editor操作と検証のためcom.unity.pipelineを導入している。ランタイム接続は有効にしていない。

## ローカル確認

プロジェクトのルートで以下を実行し、http://127.0.0.1:8765 を開く。

```sh
python3 -m http.server 8765 --bind 127.0.0.1 --directory Builds/Web
```

index.htmlをダブルクリックしてfile://で開くと、ブラウザがUnityのデータ取得を拒否する。
Webテンプレートはこの場合にローダーを起動せず、HTTP/HTTPS配信またはBuild And Runを使う案内を表示する。
Assets/WebGLTemplates内のindex.htmlはビルド用の雛形であり、直接実行しない。
