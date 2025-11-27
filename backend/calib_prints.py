import numpy as np
import cv2
import glob
import os

# --- 1. 定数設定 (計測した値に修正してください) ---
CHECKERBOARD = (9, 6) # 横9個、縦5個の交点
# 🚨 測定した正方形の一辺の実寸 (例: 16.0 mm) に変更すること！
square_size = 16.0 

# 終端基準
criteria = (cv2.TERM_CRITERIA_EPS + cv2.TERM_CRITERIA_MAX_ITER, 30, 0.001)

# ワールド座標系における3D点
objp = np.zeros((CHECKERBOARD[0] * CHECKERBOARD[1], 3), np.float32)
# Z=0 の平面上の格子点として定義
objp[:, :2] = np.mgrid[0:CHECKERBOARD[0], 0:CHECKERBOARD[1]].T.reshape(-1, 2) * square_size

objpoints = [] # 3D点 (ワールド座標)
imgpoints = [] # 2D点 (画像座標)

# 💡 変換後の画像ファイルを読み込む (例としてJPGを使用)
images = glob.glob('calib_prints/*.jpg') # HEICから変換したファイル形式に合わせる

if not images:
    print("❌ エラー: 指定されたフォルダに画像ファイルが見つかりません。パスを確認してください。")
    # Z-axisを保持した3D形式の空の配列の形状: (0, 6)
    exit()

for fname in images:
    img = cv2.imread(fname)
    
    if img is None:
        print(f"Skipping {fname}: ファイルの読み込みに失敗しました。")
        continue

    gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)

    # チェスボードのコーナーを検出
    ret, corners = cv2.findChessboardCorners(gray, CHECKERBOARD, None)

    if ret == True:
        # 3D座標リストにワールド座標を追加
        objpoints.append(objp)
        
        # 2D座標の精度を向上
        corners2 = cv2.cornerSubPix(gray, corners, (11, 11), (-1, -1), criteria)
        imgpoints.append(corners2)
        
    else:
        print(f"Warning: Corners not found in {fname}. (確認: 角が画面端で切れていないか、ピンボケがないか)")

# --- 2. キャリブレーションの実行 ---
if len(objpoints) >= 5: # 最低限の画像数チェック
    ret, mtx, dist, rvecs, tvecs = cv2.calibrateCamera(
        objpoints, imgpoints, gray.shape[::-1], None, None
    )

    if ret:
        print("\n✅ カメラキャリブレーション成功！")
        print("------------------------------------------------------------------")
        print("## 内部パラメーター (カメラ行列) ##")
        print("CAMERA_MATRIX:\n", mtx)
        
        print("\n## 歪み係数 ##")
        print("DIST_COEFFS:\n", dist)
        print("------------------------------------------------------------------")
        print("\nこれらの値をFastAPIコードに組み込んでください。")
    else:
        print("\n❌ キャリブレーション失敗。検出されたコーナー数が不足している可能性があります。")
else:
    print(f"\n❌ 処理できる画像が少なすぎます。コーナーが検出されたのは {len(objpoints)} 枚のみでした。")