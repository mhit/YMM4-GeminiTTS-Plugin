# YMM4 Gemini TTS Plugin

YukkuriMovieMaker 4 (YMM4) 向け音声合成プラグイン。Google Cloud Text-to-Speech API
経由で **Gemini 3.1 Flash TTS (preview)** を呼び出し、30 種類のネイティブボイスで
ナレーションを生成します。

## 特長

- YMM4 のキャラクター設定「声質」リストに Gemini TTS が並び、既存の VOICEPEAK や
  AivisSpeech と同じワークフローで使える
- 30 voice（Kore / Zephyr / Puck / Aoede など）から選択可能
- SSML ではなく自然言語プロンプトでスタイル指定（例: 「落ち着いたプロのナレーター調で」）
- 複数作業者が同時に使ってもライセンス上は 1 Google Cloud プロジェクトで済む

## 必要環境

- Windows 10 / 11
- YukkuriMovieMaker v4（プラグイン機構が追加された版）
- .NET 8 Desktop Runtime（YMM4 本体が同梱）
- Google Cloud プロジェクト + Text-to-Speech API 有効化
- Service Account JSON キー

## インストール

### `.ymme` パッケージから

1. リリースページから `YMM4.GeminiTTS.Plugin.ymme` をダウンロード
2. YMM4 を起動し、ファイルをウィンドウへドラッグ＆ドロップ
3. YMM4 を再起動

### 手動インストール

1. `YMM4.GeminiTTS.Plugin.dll` と依存 DLL (`Google.*.dll` 等) を
   `YMM4インストールフォルダ\user\plugin\GeminiTTS\` にコピー
2. YMM4 を再起動

## Service Account 準備

1. [Google Cloud Console](https://console.cloud.google.com/) で新規プロジェクトを作成
2. 「API とサービス」→「ライブラリ」で **Cloud Text-to-Speech API** を有効化
3. 「IAM と管理」→「サービス アカウント」で新規作成
4. ロール「**Cloud Text-to-Speech ユーザー**」を付与
5. そのサービスアカウントの「キー」タブから JSON キーを発行しローカル保存
   （例: `C:\secrets\gemini-tts-sa.json`）
6. 課金アカウントをプロジェクトに紐付け

> Gemini 3.1 Flash TTS は 2026 年 4 月時点で preview 版です。料金は変動する
> 可能性があるため、請求アラート（例: 月 $10）の設定を推奨します。

## プラグイン設定

YMM4 → 設定 → プラグイン → **Gemini TTS**:

| 項目 | 説明 |
| --- | --- |
| Service Account JSON パス | 上で保存した JSON のフルパス |
| デフォルト Voice | 新規キャラクターに割り当てる voice |
| デフォルトスタイルプロンプト | 全発話に前置する指示文 |
| サンプルレート | 24000 Hz 推奨（Gemini ネイティブ） / 48000 Hz も可 |

設定は YMM4 本体の設定保存機構（`SettingsBase<T>`）を利用するため、他の
音声プラグインと同じ場所（YMM4 の設定フォルダ配下）に自動保存されます。

## 使い方

1. YMM4 のキャラクター設定で「声質」を `Gemini - Kore` などに変更
2. セリフを入力 → 再生 / 書き出し
3. セリフ単位でスタイルを上書きしたい場合は、音声パラメータ欄の
   「Style Prompt」に指示を入力（例: `囁き声で、少しためらいながら`）

### スタイル制御の例

- `落ち着いたプロのナレーター調で、低めの声で` — 企業紹介動画向け
- `元気いっぱい、早口で` — ゲーム実況向け
- `ニュースアナウンサーのように、明瞭に` — 解説動画向け
- `[whisper]` / `[excited]` — 公式 audio tag もテキスト内で利用可

Gemini TTS は SSML を受け付けず、`pitch` / `speakingRate` も無視します。
すべてのニュアンス調整はプロンプト経由です。

## 既知の制約

- **オンライン必須**: Cloud API 呼び出しなのでネット障害時は生成不能
- **SynthID**: Gemini TTS 出力には SynthID ウォーターマークが埋め込まれます
- **preview モデル**: 予告なく料金体系・voice 一覧が変更される可能性あり
- **言語**: MVP では `ja-JP` 固定（将来の多言語対応で設定項目を追加予定）

## 開発者向けビルド手順

```powershell
# YMM4 のインストール先を環境変数で指定
$env:YMM4_DIR = "C:\Program Files\YukkuriMovieMaker"

dotnet restore
dotnet build -c Release

# プラグイン出力
dotnet publish -c Release -o .\publish
```

`.ymme` パッケージは `publish/` 配下の DLL 群を ZIP 圧縮し拡張子を `.ymme` に
変更するだけで作成できます。

## 開発スコープ（MVP）

本プラグインの MVP は以下に絞っています:

- モデル: `gemini-3.1-flash-tts-preview` のみ
- 経路: Cloud Text-to-Speech API（Gemini API 直叩きではない）
- 認証: Service Account JSON
- 言語: `ja-JP`

**非スコープ（将来拡張）**: multi-speaker、ストリーミング、Gemini 2.5 系、
感情アーク UI。

## ライセンス

MIT

## 参考リンク

- [YMM4 プラグイン作成（公式 FAQ）](https://manjubox.net/ymm4/faq/plugin/how_to_make/)
- [YMM4 音声合成プラグインの作り方（inuinu 氏）](https://zenn.dev/inuinu/articles/how2build-ymm4-synthesis-plugin)
- [Gemini-TTS in Cloud Text-to-Speech](https://docs.cloud.google.com/text-to-speech/docs/gemini-tts)
