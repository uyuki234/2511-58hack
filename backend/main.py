import cv2
import numpy as np
from fastapi import FastAPI, UploadFile, File, Response
import mediapipe as mp

app = FastAPI()

# MediaPipe 初期化
mp_face_mesh = mp.solutions.face_mesh


# ====== 人の顔の点群 ======
def get_human_points_from_bytes(image_bytes: bytes):
    nparr = np.frombuffer(image_bytes, np.uint8)
    image = cv2.imdecode(nparr, cv2.IMREAD_COLOR)

    if image is None:
        return np.empty((0, 2), dtype=np.float32)

    image_rgb = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)

    with mp_face_mesh.FaceMesh(static_image_mode=True, max_num_faces=1) as face_mesh:
        results = face_mesh.process(image_rgb)

    if not results.multi_face_landmarks:
        return np.empty((0, 2), dtype=np.float32)

    face_landmarks = results.multi_face_landmarks[0]

    points = np.array([[lm.x, lm.y] for lm in face_landmarks.landmark], dtype=np.float32)
    return points


# ====== 物体（AKAZE特徴点） ======
def get_object_points_from_bytes(image_bytes: bytes):
    nparr = np.frombuffer(image_bytes, np.uint8)
    image = cv2.imdecode(nparr, cv2.IMREAD_GRAYSCALE)

    if image is None:
        return np.empty((0, 2), dtype=np.float32)

    detector = cv2.AKAZE_create()
    keypoints = detector.detect(image, None)

    if not keypoints:
        return np.empty((0, 2), dtype=np.float32)

    points = np.array([kp.pt for kp in keypoints], dtype=np.float32)
    points /= np.array([image.shape[1], image.shape[0]], dtype=np.float32)

    return points


# ====== エンドポイント ======
@app.post("/pointcloud")
async def pointcloud(file: UploadFile = File(...)):
    image_bytes = await file.read()

    # まず人の顔を試す
    human_pts = get_human_points_from_bytes(image_bytes)

    if len(human_pts) > 0:
        # media_type="application/octet-stream" は「汎用的なバイナリデータ」という意味
        return Response(content=human_pts.tobytes(), media_type="application/octet-stream")


    # 顔がなければ物体特徴点を返す
    object_pts = get_object_points_from_bytes(image_bytes)
    return Response(content=object_pts.tobytes(), media_type="application/octet-stream")
