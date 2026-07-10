# GitHub Actions Workflows

self-hosted Windows ランナー上のローカル Unity で APK をビルドするワークフローです。
（Unity ライセンスの Secrets は不要。ランナーにインストール済みの Unity を使用）

## ワークフロー

| ファイル | ターゲット | ビルドメソッド | 出力 |
|---|---|---|---|
| `build-android-apk.yml` | Android (ARCore対応スマホ) | `CIBuild.BuildAndroid` | `NsdkSamples-Android.apk` |
| `build-quest-apk.yml` | Meta Quest 3 | `CIBuild.BuildQuest` | `NsdkSamples-Quest.apk` |

どちらも `workflow_dispatch` で手動起動でき、`main` への push（`NsdkSamples/**` 変更時）でも起動します。
APK とビルドログは Actions の Artifacts からダウンロードできます。

## フレーバーの違い（NsdkSamples/Assets/Editor/CIBuild.cs）

| 設定 | Android | Quest |
|---|---|---|
| XR ローダー (Android) | Nsdk AR Core Loader | OpenXR Loader |
| OpenXR 機能 | — | Meta Quest Support + Oculus Touch Controller Profile |
| アーキテクチャ | ARMv7 + ARM64 | ARM64 のみ |
| Scripting Backend | IL2CPP | IL2CPP |

## ランナー要件

- ラベル: `self-hosted`, `Windows`
- Unity Hub の既定パス `C:\Program Files\Unity\Hub\Editor\<version>` に
  Android Build Support (SDK/NDK/OpenJDK) 付きの Unity がインストール済みであること
- 既定バージョンは `6000.0.74f1`（`workflow_dispatch` の `unityVersion` で変更可。
  プロジェクト自体は `6000.0.58f2` で作成されている）
- Unity のライセンスはランナー上でサインイン済み（named-user license）であること

## Meta Quest ビルドに関する補足

現状の Quest フレーバーは OpenXR + Meta Quest Support を有効にした APK を生成します
（Quest 3 にインストール・起動可能）。Niantic の AR 機能（パススルーカメラ・VPS 等）を
Quest 上でフル動作させるには、公式ドキュメントの手順どおり
`com.nianticspatial.nsdk.metaquest`（nsdk-library-upm-quest3）と
XR Interaction Toolkit の導入、Meta Plugin サンプルシーンのインポートが別途必要です。
https://www.nianticspatial.com/docs/nsdk/setup/
