import cv2
import mediapipe as mp
import matplotlib.pyplot as plt
import numpy as np
import os

# --- MediaPipe初期化 ---
mp_face_mesh = mp.solutions.face_mesh

# --- 人の顔から点群を取得 ---
def get_human_points(image_path):
    image = cv2.imread(image_path)
    if image is None:
        raise FileNotFoundError(f"画像が読み込めません: {image_path}")
    image_rgb = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)

    with mp_face_mesh.FaceMesh(static_image_mode=True, max_num_faces=1) as face_mesh:
        results = face_mesh.process(image_rgb)

    if not results.multi_face_landmarks:
        print(f"[INFO] 顔が検出されませんでした: {os.path.basename(image_path)}")
        return np.empty((0, 2))

    face_landmarks = results.multi_face_landmarks[0]
    points = np.array([[lm.x, lm.y] for lm in face_landmarks.landmark])
    print(f"[INFO] 顔点群を取得 ({len(points)} 点)")
    return points

# --- 物体や風景などの点群を取得 (AKAZE版) ---
# ご提示いただいたAKAZEを使用する関数に置き換えました
def get_object_points(image_path):
    image = cv2.imread(image_path, cv2.IMREAD_GRAYSCALE)
    if image is None:
        raise FileNotFoundError(f"画像が読み込めません: {image_path}")

    detector = cv2.AKAZE_create()  # ORB_create() の代わり
    keypoints = detector.detect(image, None)
    
    if not keypoints:
        print(f"[INFO] 特徴点が検出されませんでした: {image_path}")
        plt.imshow(image, cmap='gray')
        plt.title("No Keypoints Detected")
        plt.show()
        return np.empty((0, 2))

    # 特徴点が検出された場合、キーポイントを描画して表示
    img_with_kp = cv2.drawKeypoints(image, keypoints, None, color=(0,255,0), flags=0)
    plt.imshow(img_with_kp)
    plt.title("AKAZE Keypoints")
    plt.show()

    points = np.array([kp.pt for kp in keypoints])
    # 座標を(0–1)に正規化
    points = points / np.array([[image.shape[1], image.shape[0]]])
    print(f"[INFO] 検出点数: {len(points)}")
    return points

# --- 点群を描画 ---
def show_points(points, title="2D Points"):
    if len(points) == 0:
        print(f"[WARN] 点群が空です ({title})")
        return
    plt.scatter(points[:, 0], points[:, 1], s=5)
    plt.gca().invert_yaxis() # Y軸を反転 (画像座標系準拠)
    plt.title(title)
    plt.show()


# --- メイン処理 ---
def main():
    # 入力画像
    human_path = "human.jpg"   # 人物画像
    object_path = "object.jpg" # 物体・風景画像 (フィギュアなど)

    print("=== 人の点群を取得 ===");
    human_points = get_human_points(human_path)

    print("\n=== 物体の点群を取得 (AKAZE) ===")
    object_points = get_object_points(object_path)

    # --- 結果の表示 ---
    
    # 1. 人間の顔（正規化された散布図）
    if len(human_points) > 0:
        show_points(human_points, "Human Face Landmarks (Normalized)")

    # 2. 物体（正規化された散布図）
    # (get_object_points内でキーポイント付き画像が既に表示されています)
    if len(object_points) > 0:
        show_points(object_points, "Object Feature Points (Normalized Scatter)")

    # 3. 統合表示（正規化された散布図）
    if len(human_points) > 0 or len(object_points) > 0:
        print("\n[INFO] 統合プロットを表示します...")
        plt.figure(figsize=(6,6))
        if len(object_points) > 0:
            plt.scatter(object_points[:, 0], object_points[:, 1], s=5, c='gray', label='Object (AKAZE)')
        if len(human_points) > 0:
            plt.scatter(human_points[:, 0], human_points[:, 1], s=5, c='red', label='Human (MediaPipe)')
        
        plt.gca().invert_yaxis()
        plt.legend()
        plt.title("Combined 2D Point Cloud (Human + Object)")
        plt.xlabel("X (Normalized)")
        plt.ylabel("Y (Normalized)")
        plt.show()

    # ファイルに保存
    np.save("human_points.npy", human_points)
    np.save("object_points.npy", object_points)
    print("[INFO] 点群データを保存しました (human_points.npy / object_points.npy)")

if __name__ == "__main__":
    main()