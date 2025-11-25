![title](https://topaz.dev/_next/image?url=https%3A%2F%2Fptera-publish.topaz.dev%2Fproject%2F01KAQAYNBNJZTMGZ9M2WFRN38W.png&w=1920&q=75)  

# 2511-58hack  

プロジェクト紹介ページ: https://topaz.dev/projects/045f4769cd99f4d895af  
バックエンドのREADME: [backend](/backend/README.md)  

お手軽フォト弾幕ゲーム（線香チーム） — 58ハッカソン2025  

このリポジトリは、関西学生エンジニアチームによるハッカソン作品のソースコードおよびビルド成果物をまとめたものです。  
フロントエンドは Unity（WebGL ビルド）、バックエンドは Python（FastAPI）で構成されています。  

**重要情報**  
- **Unity バージョン**: 6000.2.10f1  
- **フロントエンド**: `58Hack/`（Unity プロジェクト）  
- **ビルド成果物（Web）**: `58Hack/58hack-built/`  
- **バックエンド**: `backend/`（FastAPI + 画像処理・点群生成）  

**プロダクト概要**  
- ジャンル: 避けゲー（弾幕）  
- 対応プラットフォーム: Web (Unity)  
- 開発環境: Unity + Python (FastAPI) + C#  

**見どころポイント**  
- 圧倒的スパーク表現（GPU ベースで高速に大量の弾幕を描画）  
- 撮影した写真を点群化してゲーム内の弾幕パターンとして利用できる  
- asmdef によるアセンブリ分割でモジュール化を実現  

**紹介（抜粋）**  
+ お手軽フォト弾幕ゲーム  
+  
### 🎮 プロジェクト概要  
**ジャンル**：避けゲー（弾幕）  
**対応プラットフォーム**：web（Unity）  
**開発環境**：Unity + Python（FastAPI）+ C#  

### 🔥 見どころポイント  
- 圧倒的スパーク！！！！  
- 写真の撮り方次第で、いろいろな楽しみ方ができる  
- 多種多様な弾幕  

### 🚀 技術チャレンジ & 強み  
#### ① 弾幕はオブジェクトを使っていない  
大量の弾幕をオブジェクト化せず、GPU 側で座標計算・描画を行う設計により CPU 負荷を最小化しています。  

主な工夫:  
- Compute Shader / GPU Instancing による並列処理  
- `DrawMeshInstanced` を使った大量描画のオーバーヘッド削減  
- オブジェクト数を極限まで減らし GC や Update 負荷を回避  

メリット:  
- 軽量化、スケーラビリティ、メモリ効率の向上  

#### ② アセンブリ分割（asmdef）  
Assembly Definition Files を用いてアセンブリを分割し、依存関係の明確化と疎結合化を実現しています。  

#### ③ 撮影画像 → 点群 → 弾幕  
スマホで撮影した画像を MediaPipe / OpenCV 等で特徴点抽出 → 点群化 → Unity に送信して弾幕生成に利用します。  

画像例:  
![sample1](https://ptera-publish.topaz.dev/project/01KAQAV0HHZM733SPFVXB6E3QD.png)  
![sample2](https://ptera-publish.topaz.dev/project/01KAQAV7AYXR8BQSQ8Z3BFYST8.png)  

技術ポイント:  
- 特徴点検出（顔ランドマーク、輪郭）  
- JSON / WebSocket 経由で点群データを Unity に送信  
- Unity 側で座標に基づく弾幕生成  

メリット:  
- インタラクティブ性の向上、機械学習の応用、拡張性  

**その他の技術**  
- GitHub + Copilot を利用した自動レビューワークフロー（PR 品質向上）  

画像（ワークフロー例）:  
![workflow](https://ptera-publish.topaz.dev/project/01KAQD9MH8E3XT1J2H6TNBVE40.png)  

**困ったこと（開発での課題）**  
- Unity のコンフリクト対応  
- バグと判定されない微妙な不具合の発見/修正  
- C# と Python の float 型互換性調整  
- GitHub の容量管理（大きなアセットは gitignore）  

**使用技術**  
- Unity  
- Python  
- FastAPI  
- C#  
- MediaPipe  
- OpenCV  

**セットアップ（バックエンド）**  
- 推奨 Python: 3.11  

簡易セットアップ例（macOS, zsh）:  

```sh
cd backend
python3.11 -m venv .venv
source .venv/bin/activate
pip3 install -r requirements.txt
uvicorn main:app --reload
```

接続テスト例:  

```sh
curl --location 'http://127.0.0.1:8000/pointcloud' \
  --form 'file=@"path/to/image.png"'
```

**フロントエンド（ローカル確認）**  
Web ビルド（`58Hack/58hack-built/`）をローカル配信する例:  

```sh
cd 58Hack/58hack-built
python3 -m http.server 8000
# ブラウザで http://127.0.0.1:8000 を開く
```

**Docker**  
- `backend/` に `Dockerfile` と `docker-compose.yml` が含まれます。内容に不安がある場合は必ず中身を確認してから利用してください。  

**貢献・連絡先**  
- 変更は Pull Request でお願いします。  
- 質問や不具合報告は Issue へ。  
