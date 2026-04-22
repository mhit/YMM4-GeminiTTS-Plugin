# YMM4 Geminiナレーター プラグイン

YukkuriMovieMaker 4 (YMM4) 向け音声合成プラグイン。  
Google の **Gemini TTS API** を呼び出し、30 種類のネイティブボイスでナレーションを生成します。

---

## 特長

- **APIキー1つで即利用** — Google AI Studio で発行した API キーを貼るだけ。無料枠あり。
- **30 ボイス対応** — Kore / Zephyr / Puck / Aoede など個性豊かなボイスを選択可能
- **感情・口調プリセット** — セリフごとに「囁き声」「怒り」「アニメキャラクター」などをドロップダウンで選択
- **一括キャラクター登録** — 設定画面のボタン1つで 30 ボイス分のキャラクターを自動追加
- **生成中オーバーレイ** — 合成中は画面右下にフローティング通知を表示
- **読み辞書** — 漢字→よみがな のインライン置換で読み間違いを修正
- **Director's Notes + Audio Tag** — 公式推奨のプロンプト構造で自然な感情表現

---

## 必要環境

- Windows 10 / 11
- YukkuriMovieMaker v4
- .NET 10 Desktop Runtime（YMM4 本体が同梱）
- Google AI Studio の API キー（無料取得可）

---

## インストール

1. リリースページから `YMM4.GeminiTTS.Plugin.dll` をダウンロード
2. YMM4 プラグインフォルダにフォルダごとコピー  
   `%LocalAppData%\YukkuriMovieMaker4\user\plugin\YMM4.GeminiTTS.Plugin\`
3. YMM4 を再起動

---

## セットアップ

### 1. API キーの取得

1. [Google AI Studio](https://aistudio.google.com/) にアクセス
2. 左メニュー「Get API key」→「APIキーを作成」
3. 発行された API キーをコピー

> 無料枠：1 分 10 リクエスト / 1 日 1,500 リクエスト（2025 年時点）。  
> 超過分は従量課金。[請求アラート](https://console.cloud.google.com/billing) の設定を推奨。

### 2. プラグイン設定

YMM4 → 設定 → **Geminiナレーター**

| 項目 | 説明 |
|---|---|
| Gemini API キー | AI Studio で取得したキー |
| モデル | `gemini-2.5-flash-preview-tts`（デフォルト）/ `gemini-2.5-pro-preview-tts` |
| 言語コード | BCP-47 形式（例: `ja-JP`, `en-US`） |
| デフォルト Voice | キャラクター作成時のデフォルトボイス |
| デフォルトスタイルプロンプト | 口調「設定に従う」のセリフに適用される指示文 |
| 読み辞書 | 漢字→よみがな の置換ルール |
| タイムアウト (秒) | リクエスト上限秒数（0 以下で無制限） |

### 3. キャラクターの一括登録

設定画面の「**キャラクター一括登録**」→「全ボイスを一括登録」をクリックすると、  
30 ボイス分のキャラクターが「Geminiナレーター」グループに自動追加されます。  
（YMM4 は自動で再起動して反映されます）

---

## 使い方

### 基本

1. キャラクター設定で声質を `Zephyr (明るい・女性)` などに変更
2. セリフを入力 → 再生 / 書き出し

合成中は画面右下にオーバーレイが表示されます。

### 感情・口調の設定

セリフ編集パネルの「**口調・感情**」ドロップダウンでプリセットを選択:

| グループ | プリセット例 |
|---|---|
| 感情・口調 | 落ち着いた・プロ / 明るく元気に / 囁き声 / 悲しく / 怒り / 笑いながら / 興奮 / 真剣 / 子供っぽく |
| 用途別 | アニメキャラクター / ゲームのセリフ / バーチャルアバター・VTuber / オーディオドラマ / 表現豊かなナレーション / ブランドボイス |
| その他 | 設定に従う（デフォルト） / カスタム（自由入力） |

「**カスタム**」を選ぶとテキスト欄が有効になり、自由な指示文を入力できます。

### Audio Tag（インラインスタイル）

セリフ本文にタグを埋め込むと、その箇所の発音スタイルが変化します:

```
[whispers] 内緒話ですよ。
[excited] やった、本当にできた！
[laughs] それは面白い話だね。
```

主なタグ: `[whispers]` `[shouting]` `[excited]` `[crying]` `[laughs]` `[serious]` `[curious]`  
`[short pause]` `[medium pause]` `[long pause]`

### 読み辞書

設定画面「読み辞書」に `句=よみ` 形式で記入:

```
# 同音異義語
橋=はし

# 社名・固有名詞
CIS=シーアイエス
LLM=エルエルエム
生成AI=せいせいえーあい
```

送信テキスト内の「句」を「よみ」で置換してから API に送ります。  
重複する句がある場合は長い句が優先されます。

---

## ボイス一覧（30 種）

| ボイス名 | 表示名 | 特徴 |
|---|---|---|
| Zephyr | Zephyr (明るい・女性) | Bright |
| Puck | Puck (元気・男性) | Upbeat |
| Charon | Charon (情報的・男性) | Informative |
| Kore | Kore (しっかり・女性) | Firm |
| Fenrir | Fenrir (熱量・男性) | Excitable |
| Leda | Leda (若々しい・女性) | Youthful |
| Orus | Orus (重厚・男性) | Firm |
| Aoede | Aoede (軽やか・女性) | Breezy |
| Callirrhoe | Callirrhoe (穏やか・女性) | Easy-going |
| Autonoe | Autonoe (明朗・女性) | Bright |
| Enceladus | Enceladus (息感・男性) | Breathy |
| Iapetus | Iapetus (澄んだ・男性) | Clear |
| Umbriel | Umbriel (ゆったり・男性) | Easy-going |
| Algieba | Algieba (滑らか・男性) | Smooth |
| Despina | Despina (なめらか・女性) | Smooth |
| Erinome | Erinome (クリア・女性) | Clear |
| Algenib | Algenib (しゃがれ・男性) | Gravelly |
| Rasalgethi | Rasalgethi (解説向け・男性) | Informative |
| Laomedeia | Laomedeia (アップビート・女性) | Upbeat |
| Achernar | Achernar (柔らか・女性) | Soft |
| Alnilam | Alnilam (堅実・男性) | Firm |
| Schedar | Schedar (均質・男性) | Even |
| Gacrux | Gacrux (成熟・女性) | Mature |
| Pulcherrima | Pulcherrima (前向き・女性) | Forward |
| Achird | Achird (親しみ・男性) | Friendly |
| Zubenelgenubi | Zubenelgenubi (カジュアル・男性) | Casual |
| Vindemiatrix | Vindemiatrix (優しい・女性) | Gentle |
| Sadachbia | Sadachbia (活発・男性) | Lively |
| Sadaltager | Sadaltager (博識・男性) | Knowledgeable |
| Sulafat | Sulafat (温かい・女性) | Warm |

---

## 既知の制約

- **オンライン必須** — ネット接続が必要です
- **SSML 非対応** — Gemini TTS は SSML を受け付けません（Audio Tag で代替）
- **SynthID** — 生成音声には Google の SynthID ウォーターマークが埋め込まれます
- **Preview モデル** — 予告なく料金体系・ボイス・動作が変更される可能性があります
- **読み指定 API 非対応** — `custom_pronunciations` は Gemini TTS では効かないため、読み辞書で代替

---

## ビルド手順

```powershell
# YMM4 本体 DLL を libs/ または YMM4_DIR で参照
$env:YMM4_DIR = "C:\Program Files\YukkuriMovieMaker"

dotnet restore
dotnet build -c Release
# Release ビルドはビルド後に自動でプラグインフォルダへ展開（YMM4 が起動中なら自動終了）
```

---

## 参考リンク

- [Gemini API — 音声生成ドキュメント](https://ai.google.dev/gemini-api/docs/speech-generation?hl=ja)
- [Google AI Studio — API キー取得](https://aistudio.google.com/)
- [YMM4 プラグイン作成（公式 FAQ）](https://manjubox.net/ymm4/faq/plugin/how_to_make/)

---

## ライセンス

MIT
