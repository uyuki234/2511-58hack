import cv2
import numpy as np
from fastapi import FastAPI, UploadFile, File, Response
import mediapipe as mp
import struct

app = FastAPI()

mp_face_mesh = mp.solutions.face_mesh

# 部位ID
PART_IDS = {
    "right_eye": 1,
    "left_eye": 2,
    "right_ear": 3,
    "left_ear": 4,
    "nose": 5,
    "mouth": 6,
}

# 色ID
COLOR_IDS = {
    "red": 1,
    "green": 2,
    "blue": 3,
    "yellow": 4,
    "cyan": 5,
    "magenta": 6,
    "white": 7,
    "black": 8
}


# === 主要色を判定 ===
def classify_color(bgr):
    b, g, r = bgr.astype(int)
    if max(r, g, b) < 40:
        return "black"
    if min(r, g, b) > 200:
        return "white"

    if r > g and r > b:
        if g > b:
            return "yellow"
        else:
            return "magenta"
    if g > r and g > b:
        if r > b:
            return "yellow"
        else:
            return "cyan"
    if b > r and b > g:
        if r > g:
            return "magenta"
        else:
            return "blue"

    return "red"


# === 人の部位ごとにメッシュポイントを分類 ===
FACE_PART_LANDMARKS = {
    "right_eye":  [33, 133],
    "left_eye":   [362, 263],
    "nose":       [1, 4],
    "mouth":      [13, 14],
    "right_ear":  [234, 454],
    "left_ear":   [54, 284]
}


def get_human_points(image_bytes):
    nparr = np.frombuffer(image_bytes, np.uint8)
    image = cv2.imdecode(nparr, cv2.IMREAD_COLOR)
    if image is None:
        return b""

    h, w = image.shape[:2]
    image_rgb = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)

    with mp_face_mesh.FaceMesh(static_image_mode=True, max_num_faces=1) as fm:
        results = fm.process(image_rgb)

    if not results.multi_face_landmarks:
        return b""

    lm = results.multi_face_landmarks[0].landmark
    output_bytes = bytearray()

    # 各部位ごとに処理
    for part_name, indices in FACE_PART_LANDMARKS.items():
        part_id = PART_IDS[part_name]

        for idx in indices:
            p = lm[idx]
            x = p.x
            y = p.y
            z = p.z

            px = int(x * (w - 1))
            py = int(y * (h - 1))
            r, g, b = image_rgb[py, px] / 255.0

            # バイナリ形式で詰める (x,y,z,r,g,b,ID)
            output_bytes += struct.pack(
                "<ffffffI",  # 先頭に '<' を付けて little-endian 明示
                x, y, z, r, g, b, part_id
            )

    return bytes(output_bytes)


# === 物体の特徴点 ===
def get_object_points(image_bytes):
    nparr = np.frombuffer(image_bytes, np.uint8)
    color = cv2.imdecode(nparr, cv2.IMREAD_COLOR)

    if color is None:
        return b""

    h, w = color.shape[:2]
    gray = cv2.cvtColor(color, cv2.COLOR_BGR2GRAY)
    detector = cv2.AKAZE_create()
    kps = detector.detect(gray, None)

    output_bytes = bytearray()

    for kp in kps:
        x_px, y_px = kp.pt
        x = x_px / (w - 1)
        y = y_px / (h - 1)
        z = 0.0

        bgr = color[int(y_px), int(x_px)]
        cname = classify_color(bgr)
        cid = COLOR_IDS[cname]

        r, g, b = (bgr[::-1] / 255.0)

        output_bytes += struct.pack(
            "<ffffffI",
            x, y, z, r, g, b, cid
        )

    return bytes(output_bytes)


# === エンドポイント ===
@app.post("/pointcloud")
async def pointcloud(file: UploadFile = File(...)):
    image_bytes = await file.read()

    human = get_human_points(image_bytes)
    if len(human) > 0:
        return Response(content=human, media_type="application/octet-stream")

    obj = get_object_points(image_bytes)
    return Response(content=obj, media_type="application/octet-stream")

@app.post("/")
async def testendpoint():
    return Response(content="success", media_type="text/plain")
