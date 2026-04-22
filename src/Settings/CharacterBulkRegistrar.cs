using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using YMM4.GeminiTTS.Plugin.Voices;

namespace YMM4.GeminiTTS.Plugin.Settings;

internal static class CharacterBulkRegistrar
{
    const string GroupName = "Geminiナレーター";

    public static (int added, int skipped, string path) Register()
    {
        var path = FindCharacterSettingsPath()
            ?? throw new FileNotFoundException(
                "YMM4 のキャラクター設定ファイルが見つかりません。YMM4 を一度起動してから試してください。");

        // バックアップ
        File.Copy(path, path + ".bak", overwrite: true);

        var json = File.ReadAllText(path);
        var root = JsonNode.Parse(json)!.AsObject();
        var characters = root["Characters"]!.AsArray();

        // 登録済み Gemini ボイス名を収集
        var registered = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var ch in characters)
        {
            var voice = ch?["Voice"];
            if (voice?["API"]?.GetValue<string>() == GeminiVoiceSpeaker.ApiId)
                registered.Add(voice["Arg"]?.GetValue<string>() ?? "");
        }

        int added = 0, skipped = 0;
        foreach (var voice in VoiceCatalog.All)
        {
            if (registered.Contains(voice.Name))
            {
                skipped++;
                continue;
            }

            characters.Add(BuildCharacterNode(voice));
            added++;
        }

        if (added > 0)
        {
            var opts = new JsonSerializerOptions { WriteIndented = false };
            File.WriteAllText(path, root.ToJsonString(opts));
        }

        return (added, skipped, path);
    }

    static JsonObject BuildCharacterNode(GeminiVoice voice)
    {
        return new JsonObject
        {
            ["Name"]       = voice.DisplayName,
            ["GroupName"]  = GroupName,
            ["Color"]      = "#FF1A237E",
            ["Layer"]      = 0,
            ["KeyGesture"] = new JsonObject { ["Key"] = 0, ["Modifiers"] = 2 },
            ["Voice"]      = new JsonObject
            {
                ["API"] = GeminiVoiceSpeaker.ApiId,
                ["Arg"] = voice.Name,
            },
            ["Volume"]       = BuildAnimValue(50.0),
            ["Pan"]          = BuildAnimValue(0.0),
            ["PlaybackRate"] = 100.0,
            ["VoiceParameter"] = new JsonObject
            {
                ["$type"]            = "YMM4.GeminiTTS.Plugin.GeminiVoiceParameter, YMM4.GeminiTTS.Plugin",
                ["VoiceTone"]        = "Default",
                ["CustomInstruction"] = "",
            },
            ["ContentOffset"]                  = "00:00:00",
            ["AdditionalTime"]                 = 0.3,
            ["VoiceFadeIn"]                    = 0.0,
            ["VoiceFadeOut"]                   = 0.0,
            ["EchoIsEnabled"]                  = false,
            ["EchoInterval"]                   = 0.1,
            ["EchoAttenuation"]                = 40.0,
            ["CustomVoiceIsEnabled"]           = false,
            ["CustomVoiceIncludeSubdirectories"] = false,
            ["CustomVoiceDirectory"]           = "",
            ["CustomVoiceFileName"]            = "",
            ["AudioEffects"]                   = new JsonArray(),
            ["IsJimakuVisible"]                = true,
            ["IsJimakuLocked"]                 = false,
            ["X"]        = BuildAnimValue(0.0),
            ["Y"]        = BuildAnimValue(530.0),
            ["Z"]        = BuildAnimValue(0.0),
            ["Opacity"]  = BuildAnimValue(100.0),
            ["Zoom"]     = BuildAnimValue(100.0),
            ["Rotation"] = BuildAnimValue(0.0),
            ["JimakuFadeIn"]  = 0.0,
            ["JimakuFadeOut"] = 0.0,
            ["Blend"]             = "Normal",
            ["IsInverted"]        = false,
            ["IsAlwaysOnTop"]     = false,
            ["IsZOrderEnabled"]   = false,
            ["IsClippingWithObjectAbove"] = false,
            ["Font"]     = "メイリオ",
            ["FontSize"] = BuildAnimValue(45.0),
            ["LineHeight2"]    = BuildAnimValue(100.0),
            ["LetterSpacing2"] = BuildAnimValue(0.0),
            ["WordWrap"]  = "NoWrap",
            ["MaxWidth"]  = BuildAnimValue(1920.0),
            ["BasePoint"] = "CenterBottom",
            ["FontColor"]  = "#FFFFFFFF",
            ["Style"]      = "Border",
            ["StyleColor"] = "#FF1A237E",
            ["Bold"] = false, ["Italic"] = false, ["Underline"] = false, ["Strikethrough"] = false,
            ["IsTrimEndSpace"]      = false,
            ["IsDevidedPerCharacter"] = false,
            ["DisplayInterval"]   = 0.0,
            ["DisplayDirection"]  = "FromFirst",
            ["HideInterval"]      = 0.0,
            ["HideDirection"]     = "FromFirst",
            ["JimakuVideoEffects"] = new JsonArray(),
            ["TachieType"]               = null,
            ["TachieCharacterParameter"] = null,
            ["MouseSmooth"]   = 4,
            ["IsTachieLocked"] = false,
            ["TachieX"]        = BuildAnimValue(0.0),
            ["TachieY"]        = BuildAnimValue(0.0),
            ["TachieZ"]        = BuildAnimValue(0.0),
            ["TachieOpacity"]  = BuildAnimValue(100.0),
            ["TachieZoom"]     = BuildAnimValue(100.0),
            ["TachieRotation"] = BuildAnimValue(0.0),
            ["TachieFadeIn"]  = 0.0,
            ["TachieFadeOut"] = 0.0,
            ["TachieBlend"]           = "Normal",
            ["TachieIsInverted"]      = false,
            ["TachieIsAlwaysOnTop"]   = false,
            ["TachieIsZOrderEnabled"] = false,
            ["TachieIsClippingWithObjectAbove"] = false,
            ["TachieDefaultItemParameter"]   = null,
            ["TachieItemVideoEffects"]       = new JsonArray(),
            ["TachieDefaultFaceParameter"]   = null,
            ["TachieDefaultFaceEffects"]     = new JsonArray(),
            ["AdditionalForegroundTemplateName"] = null,
            ["AdditionalBackgroundTemplateName"] = null,
            ["VoiceItemLength"]  = 300,
            ["TachieItemLength"] = 300,
            ["VoiceItemKeyFrames"] = new JsonObject { ["Frames"] = new JsonArray(), ["Count"] = 0 },
            ["TachieItemKeyFrames"] = new JsonObject { ["Frames"] = new JsonArray(), ["Count"] = 0 },
        };
    }

    static JsonObject BuildAnimValue(double value)
    {
        var pt = (double v) => new JsonObject
        {
            ["Point"] = new JsonObject
            {
                ["X"] = v == 0.0 ? 0.0 : 1.0,
                ["Y"] = v == 0.0 ? 0.0 : 1.0,
            },
            ["ControlPoint1"] = new JsonObject { ["X"] = -0.3, ["Y"] = -0.3 },
            ["ControlPoint2"] = new JsonObject { ["X"] =  0.3, ["Y"] =  0.3 },
        };
        return new JsonObject
        {
            ["Values"]        = new JsonArray(new JsonObject { ["Value"] = value }),
            ["Span"]          = 0.0,
            ["AnimationType"] = "なし",
            ["Bezier"]        = new JsonObject
            {
                ["Points"]      = new JsonArray(pt(0.0), pt(1.0)),
                ["IsQuadratic"] = false,
            },
        };
    }

    static string? FindCharacterSettingsPath()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var base_ = Path.Combine(localAppData, "YukkuriMovieMaker4", "user", "setting");
        if (!Directory.Exists(base_)) return null;

        // 最新バージョンフォルダを選ぶ
        return Directory.EnumerateFiles(base_, "YukkuriMovieMaker.Settings.CharacterSettings.json",
                SearchOption.AllDirectories)
            .OrderByDescending(p => p)
            .FirstOrDefault();
    }
}
