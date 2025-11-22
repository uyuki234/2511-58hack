# GitHub Copilot / AI Agent Instructions (プロジェクト固有)

このファイルは、この Unity プロジェクトで AI コーディングエージェントが素早く安全に作業するための要点をまとめたものです。実際のコードとディレクトリを参照し、修正提案は既存のパターンに合わせて下さい。

- **プロジェクトの概要**: プロジェクトは Unity ベースのゲーム／アプリケーションで、主要コンポーネントは `Assets/Common`（共通データ構造・インタフェース）、`Assets/DataConnector`（外部/ローカルサーバからデータを取得するモジュール）、`Assets/MainGame`（ゲーム本体シーン・ロジック）です。

- **重要ファイル**:
  - `Assets/Common/PointsData.cs` — データモデル（`PicturePoints`, `Point`, `IDataReceiver`）を定義しています。
  - `Assets/DataConnector/DataConnector.cs` — `IDataReceiver` を実装する想定のネットワークモジュール（`UnityWebRequest` を使用）。ローカルの開発サーバー `http://localhost:8000` を想定しています。
  - `Assets/*/*.asmdef` — モジュールごとにアセンブリ定義ファイルが存在します（`Common.asmdef`, `DataConnector.asmdef`）。依存関係を壊さないように asmdef を尊重して変更してください。
  - `Packages/manifest.json` — 利用中の Unity パッケージ（テストフレームワーク等）。
  - `ProjectSettings/ProjectVersion.txt` — 推奨エディタバージョンを確認してください（このリポジトリでは `m_EditorVersion: 6000.2.10f1`）。

- **発見した実装上の注意点（要確認）**:
  - `Assets/Common/PointsData.cs` の `IDataReceiver` は `public PicturePoints GetData();` を宣言していますが、`Assets/DataConnector/DataConnector.cs` の実装は `IEnumerator<PicturePoints> IDataReceiver.GetData()` のように見え、シグネチャが一致していません。変更提案はインタフェースか実装のどちらかを修正する方向で出してください。
  - `DataConnector` は `URI = "http://localhost:8000/pointcloud"` をデフォルトにしており、ローカルAPIサーバーを前提とする動作テストが期待されます。ネットワークコードを変更する際はタイムアウト／エラー処理に注意してください。

- **開発ワークフロー（手順・コマンド例）**:
  - ローカル開発・テスト
    1. Unity Hub で `ProjectVersion.txt` に合わせた Editor を開きます。
    2. エディタ内で Play モードを使って動作確認（`MainGame` シーン等）。
  - CLI でビルドやテスト（CI 用の参考例）
    - エディタバージョンを適合させたマシンで次のように実行します（macOS/zsh 例）：

```bash
"/Applications/Unity/Hub/Editor/2024.2.10f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -quit -projectPath "$PWD" \
  -runEditorTests -testPlatform PlayMode \
  -logFile build/unity-test-log.txt
```

    - エディタパスは環境に合わせて置換してください。ローカルサーバー（`http://localhost:8000`）がテストに必要なら、CI ではモックやテストサーバーを用意してください。

- **テストと CI**:
  - `com.unity.test-framework` が `manifest.json` に含まれています。Unity Test Runner を用いて PlayMode / EditMode テストが可能です。
  - リポジトリ内に CI 定義は見つかりませんでした。CI を追加する場合、Unity のコマンドラインテストランを使って結果を収集してください。

- **コーディング規約・慣習（このプロジェクト固有）**:
  - 共通のデータ定義は `Common` 名前空間にまとめられています（参照: `Assets/Common/PointsData.cs`）。他モジュールはこの名前空間の型を直接参照します。
  - モジュール分離は `*.asmdef` を使っているため、コード移動や参照追加の際は asmdef を更新してビルドエラーを避けてください。
  - バイナリや生成物（`Library/`, `Temp/`）はソース管理対象外にするのが前提です（既にリポジトリに含まれている場合は注意）。

- **外部統合ポイント**:
  - `Assets/DataConnector` は外部API（データ提供サーバ）と通信します。既定のエンドポイントは `http://localhost:8000` です。
  - ネイティブプラグインや第三者サービスは manifest に記載は見つかりませんでした。

- **変更提案を出す際のルール**:
  - API シグネチャ（特に `IDataReceiver`）の変更は既存の実装に影響するため、PR にて影響範囲（参照する `.asmdef` と利用箇所）を明記してください。
  - 小さなバグ修正（スペル、null チェックなど）は直接 PR で送って構いませんが、インタフェース変更は事前に議論してください。

- **参照例（抜粋）**:
  - データ型: `Assets/Common/PointsData.cs`
  - ネットワーク実装: `Assets/DataConnector/DataConnector.cs`
  - パッケージ一覧: `Packages/manifest.json`
  - 推奨エディタ: `ProjectSettings/ProjectVersion.txt`

---
フィードバックをください: 明確化が必要なビルド手順や CI 要件、または特定のファイル／パターンの追加説明があれば教えてください。これに基づいて内容を反映・微調整します。
