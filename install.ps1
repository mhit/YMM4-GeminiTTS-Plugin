#Requires -Version 5.1
<#
.SYNOPSIS
    YMM4 Geminiナレーター プラグイン インストーラー

.EXAMPLE
    # 最新版をインストール
    irm https://raw.githubusercontent.com/mhit/YMM4-GeminiTTS-Plugin/main/install.ps1 | iex

    # バージョン指定インストール
    .\install.ps1 -Version 1.0.0
#>
param(
    [string] $Version = "latest"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$Repo    = "mhit/YMM4-GeminiTTS-Plugin"
$PluginDir = Join-Path $env:LOCALAPPDATA "YukkuriMovieMaker4\user\plugin\YMM4.GeminiTTS.Plugin"

function Write-Step([string]$msg) { Write-Host "  -> $msg" -ForegroundColor Cyan }
function Write-Ok([string]$msg)   { Write-Host "  OK $msg" -ForegroundColor Green }
function Write-Err([string]$msg)  { Write-Host "  !! $msg" -ForegroundColor Red; exit 1 }

Write-Host ""
Write-Host "======================================" -ForegroundColor Magenta
Write-Host "  YMM4 Geminiナレーター インストーラー" -ForegroundColor Magenta
Write-Host "======================================" -ForegroundColor Magenta
Write-Host ""

# --- YMM4 のインストール確認 ---
Write-Step "YMM4 のインストールを確認中..."
$settingDir = Join-Path $env:LOCALAPPDATA "YukkuriMovieMaker4\user\setting"
if (-not (Test-Path $settingDir)) {
    Write-Err "YukkuriMovieMaker 4 が見つかりません。先に YMM4 をインストールして一度起動してください。"
}
Write-Ok "YMM4 を検出しました"

# --- 最新リリースの取得 ---
Write-Step "GitHub から最新バージョンを取得中..."
$apiUrl = if ($Version -eq "latest") {
    "https://api.github.com/repos/$Repo/releases/latest"
} else {
    "https://api.github.com/repos/$Repo/releases/tags/v$Version"
}

try {
    $headers = @{ "User-Agent" = "YMM4-GeminiTTS-Installer" }
    $release = Invoke-RestMethod -Uri $apiUrl -Headers $headers
} catch {
    Write-Err "GitHub API の取得に失敗しました: $_"
}

$tagName  = $release.tag_name
$asset    = $release.assets | Where-Object { $_.name -like "*.zip" } | Select-Object -First 1
if (-not $asset) {
    Write-Err "リリース $tagName に zip アセットが見つかりません"
}

Write-Ok "バージョン $tagName を発見"

# --- 既存バージョンの確認 ---
$existingDll = Join-Path $PluginDir "YMM4.GeminiTTS.Plugin.dll"
if (Test-Path $existingDll) {
    $current = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($existingDll).FileVersion
    Write-Host "  現在のバージョン: $current" -ForegroundColor Yellow
    if ("v$current" -eq $tagName) {
        Write-Host "  最新版がインストール済みです。" -ForegroundColor Green
        Write-Host ""
        exit 0
    }
}

# --- YMM4 が起動中なら終了 ---
$ymm4Procs = Get-Process -Name "YukkuriMovieMaker" -ErrorAction SilentlyContinue
if ($ymm4Procs) {
    Write-Step "YMM4 を終了中..."
    $ymm4Procs | Stop-Process -Force
    Start-Sleep -Seconds 1
    Write-Ok "YMM4 を終了しました"
}

# --- ダウンロード ---
Write-Step "ダウンロード中: $($asset.name)"
$tmpZip = Join-Path $env:TEMP $asset.name
Invoke-WebRequest -Uri $asset.browser_download_url -OutFile $tmpZip -UseBasicParsing
Write-Ok "ダウンロード完了"

# --- インストール ---
Write-Step "プラグインフォルダに展開中..."
New-Item -ItemType Directory -Path $PluginDir -Force | Out-Null
Expand-Archive -Path $tmpZip -DestinationPath $PluginDir -Force
Remove-Item $tmpZip -Force
Write-Ok "インストール完了: $PluginDir"

Write-Host ""
Write-Host "  ✓ Geminiナレーター $tagName のインストールが完了しました！" -ForegroundColor Green
Write-Host "  YMM4 を起動して、設定から API キーを入力してください。" -ForegroundColor Green
Write-Host ""
