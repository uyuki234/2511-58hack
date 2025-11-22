import cv2
import numpy as np
from fastapi import FastAPI, UploadFile, File, Response
import mediapipe as mp

app = FastAPI()

# MediaPipe 初期化
mp_face_mesh = mp.solutions.face_mesh


# ====== 人の顔の点群（色付き） ======
def get_human_points_from_bytes(image_bytes: bytes):
    nparr = np.frombuffer(image_bytes, np.uint8)
    image = cv2.imdecode(nparr, cv2.IMREAD_COLOR)

    if image is None:
        return np.empty((0, 6), dtype=np.float32)

    height, width = image.shape[:2]
    image_rgb = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)

    with mp_face_mesh.FaceMesh(static_image_mode=True, max_num_faces=1) as face_mesh:
        results = face_mesh.process(image_rgb)

    if not results.multi_face_landmarks:
        return np.empty((0, 6), dtype=np.float32)

    face_landmarks = results.multi_face_landmarks[0]

    pts = []
    for lm in face_landmarks.landmark:
        x = float(lm.x)  # normalized 0..1
        y = float(lm.y)
        z = float(lm.z)  # relative depth (can be negative)
        px = int(np.clip(round(x * (width - 1)), 0, width - 1))
        py = int(np.clip(round(y * (height - 1)), 0, height - 1))
        r, g, b = image_rgb[py, px].astype(np.float32) / 255.0
        pts.append([x, y, z, r, g, b])

    return np.array(pts, dtype=np.float32)


# ====== 物体（AKAZE特徴点・色付き） ======
def get_object_points_from_bytes(image_bytes: bytes):
    nparr = np.frombuffer(image_bytes, np.uint8)
    color = cv2.imdecode(nparr, cv2.IMREAD_COLOR)

    if color is None:
        return np.empty((0, 6), dtype=np.float32)

    gray = cv2.cvtColor(color, cv2.COLOR_BGR2GRAY)
    height, width = color.shape[:2]

    detector = cv2.AKAZE_create()
    keypoints = detector.detect(gray, None)

    if not keypoints:
        return np.empty((0, 6), dtype=np.float32)

    pts = []
    for kp in keypoints:
        x_px, y_px = kp.pt
        x = float(x_px / (width - 1))
        y = float(y_px / (height - 1))
        px = int(np.clip(round(x_px), 0, width - 1))
        py = int(np.clip(round(y_px), 0, height - 1))
        b, g, r = color[py, px].astype(np.float32) / 255.0
        z = 0.0
        pts.append([x, y, z, r, g, b])

    return np.array(pts, dtype=np.float32)


# ====== エンドポイント ======
@app.post("/pointcloud")
async def pointcloud(file: UploadFile = File(...)):
    image_bytes = await file.read()

    # まず人の顔を試す
    human_pts = get_human_points_from_bytes(image_bytes)

    if human_pts.shape[0] > 0:
        return Response(content=human_pts.tobytes(), media_type="application/octet-stream")

    # 顔がなければ物体特徴点を返す
    object_pts = get_object_points_from_bytes(image_bytes)
    return Response(content=object_pts.tobytes(), media_type="application/octet-stream")

@app.get("/")
async def testendpoint():
    return Response(content="success", media_type="text/plain")
