# YMM4 Gemini TTS Plugin

YukkuriMovieMaker 4 (YMM4) 向け音声合成プラグイン。Google Cloud Text-to-Speech API
経由で **Gemini 3.1 Flash TTS (preview)** を呼び出し、30 種類のネイティブボイスで
ナレーションを生成します。

## 特長

- YMM4 のキャラクター設定「声質」リストに Gemini TTS が並び、既存の VOICEPEAK や
  AivisSpeech と同じワークフローで使える
- 30 voice（Kore / Zephyr / Puck / Aoede など）から選択可能
- 自然言語プロンプト + 公式 audio tag でスタイル指定（SSML 非対応）
- Cloud TTS の effects_profile_id による再生環境別音質チューニング対応
- 70+ 言語対応（Gemini 3.1 Flash TTS のサポート言語に準拠）
- `TextToSpeechClient` はキャッシュされ、台本一括生成でも gRPC 再接続しない
- リージョン別エンドポイント切り替え可能（低レイテンシ運用）
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
| Service Account JSON パス | 上で保存した JSON のフルパス（未設定だと Voice 一覧が非表示） |
| Endpoint | リージョン別ホスト（空欄＝グローバル既定） |
| 言語コード | BCP-47 (例: `ja-JP`, `en-US`, `zh-CN`) |
| サンプルレート | 24000 Hz 推奨（Gemini ネイティブ） / 48000 Hz も可（API 側で再サンプリング） |
| デフォルト Voice | 新規キャラクターに割り当てる voice |
| Effects Profile | 再生環境最適化（下記参照）。セミコロン区切りで複数指定可 |
| デフォルトスタイルプロンプト | 全発話に前置する指示文（system instruction） |
| リクエストタイムアウト | 1 リクエストあたりの上限秒数（ハング防止） |

設定画面の「接続テスト / Voice 一覧」で:

- **テスト合成**: 一時フォルダに短い WAV を書き出して API 到達性を確認
- **Voice 一覧を取得**: Cloud TTS の `ListVoices` を叩いて現在有効な voice 名を一覧表示
  （この結果が本プラグインの内蔵カタログと食い違う場合は Google 側の更新あり）

設定は YMM4 本体の設定保存機構（`SettingsBase<T>`）を利用するため、他の
音声プラグインと同じ場所（YMM4 の設定フォルダ配下）に自動保存されます。

## 使い方

1. YMM4 のキャラクター設定で「声質」を `Gemini - Kore` などに変更
2. セリフを入力 → 再生 / 書き出し
3. セリフ単位でスタイルを上書きしたい場合は、音声パラメータ欄の
   「Style Prompt」に指示を入力（例: `囁き声で、少しためらいながら`）

### スタイル制御の 2 つの経路

Gemini TTS は SSML を受け付けず、`pitch` / `speakingRate` / `volumeGainDb` も
原則無視します。ニュアンス調整の正攻法は以下の 2 通り:

**(A) system instruction（= スタイルプロンプト）**
発話テキストとは別フィールド（`SynthesisInput.Prompt`）で渡す高レベル指示。
本プラグインではプラグイン設定の「デフォルトスタイルプロンプト」とセリフ毎の
`Style Prompt` がここにマッピングされます。

例:
- `落ち着いたプロのナレーター調で、低めの声で` — 企業紹介動画向け
- `元気いっぱい、早口で` — ゲーム実況向け
- `ニュースアナウンサーのように、明瞭に` — 解説動画向け

**(B) inline audio tag**
セリフ本文中に `[タグ]` を埋め込む。Gemini 側が該当箇所の発話スタイルを
変化させます。公式ドキュメントで明示されている主なタグ:

| カテゴリ | タグ |
| --- | --- |
| 非言語音 | `[sigh]` `[laughing]` `[laughs]` `[giggles]` `[gasp]` `[uhm]` `[sighs]` |
| スタイル | `[whispering]` `[whispers]` `[shouting]` `[robotic]` `[sarcastic]` `[sarcasm]` `[extremely fast]` `[serious]` `[mischievously]` |
| 感情 | `[scared]` `[curious]` `[bored]` `[excited]` `[amazed]` `[panicked]` `[tired]` `[crying]` `[trembling]` |
| 間 | `[short pause]` `[medium pause]` `[long pause]` |

例:
```
[whispering] 内緒話。[long pause] 聞こえましたか？
[excited] やった、本当にできた！
```

> 網羅的なリストは公式に公開されていません。動画ナレーション向けには
> `[short pause]` `[long pause]` `[sighs]` などが特に有用です。試行で見つけてください。

### 読み辞書（同音異義語・新語の制御）

Gemini TTS は Cloud TTS の `custom_pronunciations` フィールドを **無視** するため
（2026 年 4 月現在、これは Chirp 3: HD 専用機能）、漢字の読み間違いを直す方法は
「テキストを送る前に書き換える」インライン置換が唯一の正攻法です。

本プラグインは設定画面「読み辞書」にある多行エディタでユーザー辞書を定義できます。
書式は 1 行 1 エントリの `句=よみ`、`#` で始まる行はコメント:

```
# 同音異義語
橋=はし
端=はし
箸=はし

# 社名・固有名詞（カタカナ固定読みを強制）
アダマスオクタ=アダマスオクタ
CIS=シーアイエス

# 最新ワード
生成AI=せいせいえーあい
LLM=エルエルエム
```

合成前にテキスト中の「句」を「よみ」で置換します。`東京都` と `東京` のように
重複する場合は **長い句を優先** するので登録順は気にせず追加できます。

よみ側はひらがな・カタカナを基本とし、最終的に Gemini が読み上げるのは置換後の
テキストそのものです。アクセントを明示したい場合は `｜` や `／` といった記号は
効きません — 代わりに Prompt の方で「〇〇 は "ま↑え" のように平板で読んで」のように
自然言語指示を書く方が確実です。

この辞書は **プロジェクト横断のグローバル設定**です。特定の動画だけで異なる読みを
使いたい場合はセリフ本文を直接カタカナで書き下してください。

### Effects Profile (再生環境別音質)

再生先デバイス向けに最適化された後処理を Cloud TTS 側でかけます。設定画面の
「Effects Profile」に以下の ID（セミコロン区切りで複数可）を入れます:

| ID | 用途 |
| --- | --- |
| `headphone-class-device` | **動画ナレーション推奨**。視聴者がイヤホン/ヘッドホンで聞く前提 |
| `large-home-entertainment-class-device` | リビングのテレビ・サウンドバー |
| `medium-bluetooth-speaker-class-device` | 中型 Bluetooth スピーカー |
| `small-bluetooth-speaker-class-device` | 小型 Bluetooth スピーカー |
| `large-automotive-class-device` | 車載スピーカー |
| `handset-class-device` | スマホ内蔵スピーカー |
| `wearable-class-device` | スマートウォッチ等 |
| `telephony-class-application` | 電話品質（ローファイ効果を狙う用途にも） |

デフォルトは `headphone-class-device`。

## 既知の制約

- **オンライン必須**: Cloud API 呼び出しなのでネット障害時は生成不能。
  バックアップとして VOICEPEAK / AivisSpeech 等を残すことを推奨。
- **SynthID**: Gemini TTS 出力には SynthID ウォーターマークが埋め込まれます。
- **preview モデル**: 予告なく料金体系・voice 一覧・audio tag が変更される可能性あり。
- **ペイロード制限**: `text` と `prompt` 合算で 8000 バイト、各フィールド 4000 バイト。
  長文は YMM4 のセリフ分割で自然に収まりますが、1 セリフが巨大なら分割してください。
- **出力長制限**: 1 リクエストあたり約 655 秒（11 分）。
- **SSML 非対応**: classic voice 用の SSML はこの経路では使えません（audio tag で代替）。
- **Multi-speaker（2 話者）**: API としては可能ですが本プラグインは MVP で未対応。

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

**実装済みの API 機能**: `SynthesisInput.Prompt` による system instruction / 30 voice
カタログ / inline audio tag / effects_profile_id / リージョン別エンドポイント /
`ListVoices` ベースの voice 検証 / クライアントキャッシュ / リクエストタイムアウト /
同時実行制限（SemaphoreSlim）/ ユーザー辞書によるインライン読み置換（漢字→よみがな）。

**非スコープ（将来拡張）**: multi-speaker 対話、ストリーミング合成
（`StreamingSynthesize`）、Gemini 2.5 系モデル切替、感情アーク UI、Long-audio 合成、
バッチ API。

> `CustomPronunciations` 経由のネイティブ読み指定は **Gemini TTS では効かない**
> （Chirp 3: HD 専用機能）ため、本プラグインではインライン置換で代替しています。
> Google が将来 Gemini TTS でもサポートした場合、本プラグインもオプトインで
> 追加予定です。

## ライセンス

MIT

## 参考リンク

- [YMM4 プラグイン作成（公式 FAQ）](https://manjubox.net/ymm4/faq/plugin/how_to_make/)
- [YMM4 音声合成プラグインの作り方（inuinu 氏）](https://zenn.dev/inuinu/articles/how2build-ymm4-synthesis-plugin)
- [Gemini-TTS in Cloud Text-to-Speech](https://docs.cloud.google.com/text-to-speech/docs/gemini-tts)
- [Audio profiles (effects_profile_id)](https://docs.cloud.google.com/text-to-speech/docs/audio-profiles)
- [Regional endpoints](https://docs.cloud.google.com/text-to-speech/docs/endpoints)
- [Gemini API speech generation (audio tag 拡張リスト)](https://ai.google.dev/gemini-api/docs/speech-generation)
