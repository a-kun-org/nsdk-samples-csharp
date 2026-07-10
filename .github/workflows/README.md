# GitHub Actions Workflows

このディレクトリには GameCI (game-ci/unity-builder@v4) を使った APK ビルド用ワークフローが含まれています。

## ワークフロー

| ファイル | ターゲット | 出力 |
|---|---|---|
| `build-android-apk.yml` | Android (ARCore対応スマホ) | `NsdkSamples-Android.apk` |
| `build-quest-apk.yml` | Meta Quest (Android) | `NsdkSamples-Quest.apk` |

どちらも `workflow_dispatch` で手動起動でき、`main` への push（`NsdkSamples/**` 変更時）でも起動します。

## 事前準備 — リポジトリの Secrets

GameCI で Unity ビルドを行うには、以下の Secrets をリポジトリに設定する必要があります。

| Secret | 用途 |
|---|---|
| `UNITY_LICENSE` | Personal ライセンスの `.ulf` の中身。手順は [GameCI docs (Personal)](https://game.ci/docs/github/activation) 参照。 |
| `UNITY_EMAIL` | Unity アカウントのメールアドレス（Pro/Plusライセンス、または Personal 認証時に使用） |
| `UNITY_PASSWORD` | 上記アカウントのパスワード |
| `UNITY_SERIAL` | Pro/Plus ライセンスのシリアル（Personal ライセンス使用時は不要） |

Personal ライセンス取得手順:

1. ローカルの Unity Hub で Personal ライセンスを取得し、`~/.local/share/unity3d/Unity/Unity_lic.ulf` (Linux/mac) / `%APPDATA%\Unity\Unity_lic.ulf` (Windows) の内容をコピー。
2. リポジトリ Settings → Secrets and variables → Actions → `New repository secret` で `UNITY_LICENSE` に貼り付け。
3. `UNITY_EMAIL` / `UNITY_PASSWORD` も同様に登録。

## Meta Quest ビルドに関する補足

現状のプロジェクトには **Meta XR プラグイン** および **Oculus XR プロバイダ** が含まれていません。Niantic Spatial SDK のドキュメント通り、Quest 上で正しく動作させるには次の追加設定が必要です。

- `Packages/manifest.json` に Niantic Spatial Meta プラグインおよび `com.unity.xr.oculus` を追加
- XR Plug-in Management で Android ターゲットの Oculus プロバイダを有効化
- Quest 用のサンプルシーンをインポート

現状の `build-quest-apk.yml` は Android APK を Quest 向け命名で生成するだけなので、Quest 実機で XR 動作させるには上記の追加設定を先に済ませてください。

## Unity バージョン

`ProjectVersion.txt` は `6000.0.58f2` を指しています。GameCI が該当エディタイメージを公開していない場合は、`workflow_dispatch` の `unityVersion` パラメータで別バージョン（例: `6000.0.58f2` に近い LTS）を指定してください。
