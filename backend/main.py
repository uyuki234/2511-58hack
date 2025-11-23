import cv2
import numpy as np
from fastapi import FastAPI, UploadFile, File, Response
import mediapipe as mp
import struct

app = FastAPI()

mp_face_mesh = mp.solutions.face_mesh

# ============================
# 6部位の固定 ID
# ============================
PART_ID = {
    "righteye": 1,
    "lefteye": 2,
    "nose": 3,
    "mouth": 4,
    "rightear": 5,
    "leftear": 6,
}

# ============================
# 468点 → 6部位分類
# ============================
RIGHT_EYE_IDX = set([33, 133, 160, 158, 159, 144, 145, 153])
LEFT_EYE_IDX  = set([362, 263, 387, 385, 386, 373, 374, 380])
NOSE_IDX = set([1, 2, 98, 97, 327, 168])
MOUTH_IDX = set([13, 14, 308, 324, 78, 82, 87])

RIGHT_EAR_IDX = set([234, 93, 132, 58])
LEFT_EAR_IDX  = set([454, 323, 361, 288])


def classify_part(i):
    """468点を6部位へ分類"""
    if i in RIGHT_EYE_IDX: return "righteye"
    if i in LEFT_EYE_IDX:  return "lefteye"
    if i in NOSE_IDX:      return "nose"
    if i in MOUTH_IDX:     return "mouth"
    if i in RIGHT_EAR_IDX: return "rightear"
    if i in LEFT_EAR_IDX:  return "leftear"

    # その他の点 → 左右耳へ割り振り
    return "rightear" if i < 234 else "leftear"


# ============================
# 彩度を上げる処理
# ============================
def boost_saturation(bgr, factor=2.5):
    """BGR → 彩度ブースト → RGB(0〜1)"""
    bgr = np.array(bgr, dtype=np.uint8).reshape(1, 1, 3)
    hsv = cv2.cvtColor(bgr, cv2.COLOR_BGR2HSV).astype(np.float32)

    # S（彩度）を増加
    hsv[..., 1] *= factor
    hsv[..., 1] = np.clip(hsv[..., 1], 0, 255)

    rgb = cv2.cvtColor(hsv.astype(np.uint8), cv2.COLOR_HSV2RGB)
    r, g, b = rgb[0, 0]
    return r / 255.0, g / 255.0, b / 255.0


# ============================
# 人間 → 468点 点群出力
# ============================
def get_human_points(image_bytes):
    nparr = np.frombuffer(image_bytes, np.uint8)
    img = cv2.imdecode(nparr, cv2.IMREAD_COLOR)
    if img is None:
        return None

    h, w = img.shape[:2]
    img_rgb = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)

    with mp_face_mesh.FaceMesh(
        static_image_mode=True,
        max_num_faces=1,
        refine_landmarks=True
    ) as fm:
        results = fm.process(img_rgb)

    if not results.multi_face_landmarks:
        return None

    face = results.multi_face_landmarks[0]
    output = bytearray()

    for i, lm in enumerate(face.landmark):
        px = int(lm.x * (w - 1))
        py = int(lm.y * (h - 1))

        # 画像からそのまま BGR 取得
        bgr = img[py, px]
        r, g, b = boost_saturation(bgr)  # 彩度アップ版

        # 部位 ID
        part = classify_part(i)
        ID = PART_ID[part]

        # 座標
        x = lm.x * w
        y = lm.y * h
        z = lm.z

        # 28byte
        output += struct.pack(
            "ffffffI",
            float(x), float(y), float(z),
            float(r), float(g), float(b),
            int(ID)
        )

    return bytes(output)


# ============================
# 物体（元のまま＋彩度ブースト）
# ============================
def classify_color(bgr):
    b, g, r = bgr.astype(int)
    if max(r, g, b) < 40: return "black"
    if min(r, g, b) > 200: return "white"
    if r > g and r > b:
        return "yellow" if g > b else "magenta"
    if g > r and g > b:
        return "yellow" if r > b else "cyan"
    if b > r and b > g:
        return "magenta" if r > g else "blue"
    return "red"


COLOR_IDS = {
    "red": 1, "green": 2, "blue": 3, "yellow": 4,
    "cyan": 5, "magenta": 6, "white": 7, "black": 8
}


def get_object_points(image_bytes):
    nparr = np.frombuffer(image_bytes, np.uint8)
    color = cv2.imdecode(nparr, cv2.IMREAD_COLOR)
    if color is None:
        return b""

    h, w = color.shape[:2]
    gray = cv2.cvtColor(color, cv2.COLOR_BGR2GRAY)
    detector = cv2.AKAZE_create()
    kps = detector.detect(gray, None)

    output = bytearray()

    for kp in kps:
        x_px, y_px = kp.pt
        x = x_px / (w - 1)
        y = y_px / (h - 1)
        z = 0.0

        bgr = color[int(y_px), int(x_px)]
        r, g, b = boost_saturation(bgr)  # 彩度アップ

        cname = classify_color(bgr)
        cid = COLOR_IDS[cname]

        output += struct.pack(
            "ffffffI",
            float(x), float(y), float(z),
            float(r), float(g), float(b),
            int(cid)
        )

    return bytes(output)


# ============================
# API
# ============================
@app.post("/pointcloud")
async def pointcloud(file: UploadFile = File(...)):
    image_bytes = await file.read()

    human = get_human_points(image_bytes)
    if human is not None:
        return Response(content=human, media_type="application/octet-stream")

    obj = get_object_points(image_bytes)
    return Response(content=obj, media_type="application/octet-stream")
