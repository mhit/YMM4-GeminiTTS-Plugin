using System;
using System.Collections.Generic;
using System.Linq;

namespace YMM4.GeminiTTS.Plugin.Synthesis;

/// <summary>
/// User-editable kanji→yomigana dictionary for overriding Gemini TTS pronunciation.
/// </summary>
/// <remarks>
/// <para>
/// Gemini 3.1 Flash TTS does <b>not</b> honor <see cref="Google.Cloud.TextToSpeech.V1.CustomPronunciations"/>
/// (that feature is scoped to Chirp 3: HD voices). The only reliable knob for
/// kanji-heavy Japanese narration — company names, technical jargon, recent
/// loanwords, and homonyms like 橋/箸/端 — is to rewrite the literal characters
/// in the <c>text</c> field before the request goes out. Gemini reads what we
/// give it.
/// </para>
/// <para>
/// The dictionary is stored as a multi-line string in settings, one entry per
/// line as <c>phrase=yomi</c>. Blank lines and <c>#</c>-prefixed comments are
/// ignored. Substitutions use longest-phrase-first matching so <c>東京都</c>
/// wins over <c>東京</c>.
/// </para>
/// </remarks>
public static class YomiDictionary
{
    public readonly record struct Entry(string Phrase, string Yomi);

    /// <summary>Parse the raw multi-line config into concrete entries.</summary>
    public static IReadOnlyList<Entry> Parse(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return Array.Empty<Entry>();

        var entries = new List<Entry>();
        foreach (var rawLine in raw.Split('\n'))
        {
            var line = rawLine.Trim().TrimEnd('\r');
            if (line.Length == 0 || line[0] == '#') continue;

            var eq = line.IndexOf('=');
            if (eq <= 0 || eq == line.Length - 1) continue;

            var phrase = line[..eq].Trim();
            var yomi = line[(eq + 1)..].Trim();
            if (phrase.Length == 0 || yomi.Length == 0) continue;

            entries.Add(new Entry(phrase, yomi));
        }
        return entries;
    }

    /// <summary>
    /// Rewrite every occurrence of each <c>Phrase</c> in <paramref name="text"/>
    /// with its corresponding <c>Yomi</c>. Longest phrases are applied first so
    /// overlapping keys (東京 / 東京都) don't clobber each other.
    /// </summary>
    public static string Apply(string text, IReadOnlyList<Entry> entries)
    {
        if (string.IsNullOrEmpty(text) || entries.Count == 0) return text;

        foreach (var entry in entries.OrderByDescending(e => e.Phrase.Length))
            text = text.Replace(entry.Phrase, entry.Yomi);

        return text;
    }

    /// <summary>Convenience: parse + apply in one call.</summary>
    public static string Apply(string text, string? rawDictionary) =>
        Apply(text, Parse(rawDictionary));
}
