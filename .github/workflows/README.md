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
| OpenXR 機能 | — | Meta Quest Support / Oculus Touch / NSDK Meta Integration / NSDK Meta AR Camera (Passthrough) |
| シーン | Scenes in Build（スマホAR用） | NSDK Meta Plugin サンプル（Home 先頭、Prepare ステップでインポート） |
| アーキテクチャ | ARMv7 + ARM64 | ARM64 のみ |
| minSdkVersion | 24（プロジェクト設定） | 32 |
| Scripting Backend | IL2CPP | IL2CPP |

Quest ジョブは、まず `.github/ci/Enable-QuestPackages.ps1` で manifest に Quest 用パッケージ
（`com.nianticspatial.nsdk.metaquest` + XRI + Meta scoped registry）を注入してから、Unity を 2 回起動します。
1 回目（`CIBuild.PrepareQuest`）で Meta Plugin の Samples をインポートし（ドメインリロード境界のため分離）、
2 回目（`CIBuild.BuildQuest`）で XR 設定を切り替えてビルドします。

コミットされた manifest は upstream と同一構成です。Meta XR SDK（`com.meta.xr.sdk.core`）の
ビルドフックは Meta 系ローダーを使わない Android ビルドを失敗させるため、
スマホ AR フレーバーには Meta 系パッケージを一切含めません（Android ジョブは
前回 Quest 実行の残骸 `Assets/Samples` も削除してからビルドします）。

## ランナー要件

- ラベル: `self-hosted`, `Windows`
- Unity Hub の既定パス `C:\Program Files\Unity\Hub\Editor\<version>` に
  Android Build Support (SDK/NDK/OpenJDK) 付きの Unity がインストール済みであること
- 既定バージョンは `6000.0.74f1`（`workflow_dispatch` の `unityVersion` で変更可。
  プロジェクト自体は `6000.0.58f2` で作成されている）
- Unity のライセンスはランナー上でサインイン済み（named-user license）であること

## Meta Quest ビルドに関する補足

Quest フレーバーは公式ドキュメントの手順に沿って
`com.nianticspatial.nsdk.metaquest`（nsdk-library-upm-quest3）+ XR Interaction Toolkit を導入し、
Meta Plugin のサンプルシーンをビルドします（パススルーカメラ等の NSDK 機能に対応）。
Meta の依存パッケージ（MRUK 等）は scoped registry `https://npm.developer.oculus.com` から解決します。
VPS 等の認証が必要な機能を実機で使うには Niantic Spatial の Developer Token が別途必要です。
https://www.nianticspatial.com/docs/nsdk/setup/
