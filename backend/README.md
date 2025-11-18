# 環境
- venv
- python3 3.13.9_1

# 注意：Dockerは適当にコピペしただけ

# 環境構築
- ターミナルにて
- 注意：python3のバージョン3.11 で
- macの方はこれでインストール可能
  - `brew install python@3.11`
```sh
cd backend
python3.11 -m venv .venv
source .venv/bin/activate
pip3 install -r requirements.txt
```
# 起動
```sh
uvicorn main:app --reroad
```

# 接続テスト
- openCVに対応している画像形式であれば全て対応
```sh
curl --location 'http://127.0.0.1:8000/pointcloud' \
--form 'file=@"path/to/image.png"'
```