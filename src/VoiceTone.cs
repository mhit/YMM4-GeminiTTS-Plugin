using System.ComponentModel.DataAnnotations;

namespace YMM4.GeminiTTS.Plugin;

/// <summary>
/// セリフごとに選択できる話し方プリセット。
/// 感情・口調グループは Audio Tag を組み合わせ、
/// 用途別グループは Director's Notes で演技スタイルを制御する。
/// </summary>
public enum VoiceTone
{
    // ── デフォルト ─────────────────────────────
    [Display(Name = "設定に従う（デフォルト）")]
    Default,

    // ── 感情・口調 ────────────────────────────
    [Display(Name = "【感情】落ち着いた・プロ")]
    Professional,

    [Display(Name = "【感情】明るく元気に")]
    Cheerful,

    [Display(Name = "【感情】囁き声・ひそひそ")]
    Whisper,

    [Display(Name = "【感情】悲しく・しんみり")]
    Sad,

    [Display(Name = "【感情】怒り気味・強め")]
    Angry,

    [Display(Name = "【感情】笑いながら・楽しそう")]
    Laughing,

    [Display(Name = "【感情】興奮・ワクワク")]
    Excited,

    [Display(Name = "【感情】真剣・きっぱり")]
    Serious,

    [Display(Name = "【感情】子供っぽく・無邪気に")]
    Childlike,

    // ── 用途別プリセット ─────────────────────────
    [Display(Name = "【用途】アニメキャラクター")]
    AnimeCharacter,

    [Display(Name = "【用途】ゲームのセリフ")]
    GameDialogue,

    [Display(Name = "【用途】バーチャルアバター / VTuber")]
    VirtualAvatar,

    [Display(Name = "【用途】オーディオドラマ")]
    AudioDrama,

    [Display(Name = "【用途】表現豊かなナレーション")]
    ExpressiveNarration,

    [Display(Name = "【用途】ブランドボイス")]
    BrandVoice,

    // ── カスタム ──────────────────────────────
    [Display(Name = "カスタム（下の入力欄を使用）")]
    Custom,
}
