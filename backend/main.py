import cv2
import numpy as np
from fastapi import FastAPI, UploadFile, File, Response
import mediapipe as mp
import struct

app = FastAPI()

mp_face_mesh = mp.solutions.face_mesh

# -------------------------
# 6部位の固定 ID と色
# -------------------------
PART_ID = {
    "righteye": 1,
    "lefteye": 2,
    "nose": 3,
    "mouth": 4,
    "rightear": 5,
    "leftear": 6,
}

PART_COLOR = {
    "righteye": (1.0, 0.0, 0.0),     # red
    "lefteye": (0.0, 1.0, 0.0),      # green
    "nose": (0.0, 0.0, 1.0),         # blue
    "mouth": (1.0, 1.0, 0.0),        # yellow
    "rightear": (1.0, 0.0, 1.0),     # magenta
    "leftear": (0.0, 1.0, 1.0),      # cyan
}

# =============================
# 468点を6部位に分類（精密モデル）
# =============================

RIGHT_EYE_IDX = set([33, 133, 160, 158, 159, 144, 145, 153])
LEFT_EYE_IDX  = set([362, 263, 387, 385, 386, 373, 374, 380])
NOSE_IDX = set([1, 2, 98, 97, 327, 168])
MOUTH_IDX = set([13, 14, 308, 324, 78, 82, 87])

RIGHT_EAR_IDX = set([234, 93, 132, 58])
LEFT_EAR_IDX  = set([454, 323, 361, 288])

def classify_part(i):
    """468点すべてを6部位に分類"""
    if i in RIGHT_EYE_IDX: return "righteye"
    if i in LEFT_EYE_IDX:  return "lefteye"
    if i in NOSE_IDX:      return "nose"
    if i in MOUTH_IDX:     return "mouth"
    if i in RIGHT_EAR_IDX: return "rightear"
    if i in LEFT_EAR_IDX:  return "leftear"

    # 残り点 → 左右耳に均等分類
    return "rightear" if i < 234 else "leftear"


# =============================
# 人間 → 468点（28byte）
# =============================
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

    output = bytearray()
    face = results.multi_face_landmarks[0]

    for i, lm in enumerate(face.landmark):
        x = lm.x * w
        y = lm.y * h
        z = lm.z

        part = classify_part(i)

        ID = PART_ID[part]
        r, g, b = PART_COLOR[part]   # ★ float(0〜1) に統一済み

        # f f f f f f I → 28 byte
        output += struct.pack(
            "ffffffI",
            float(x), float(y), float(z),
            float(r), float(g), float(b),
            int(ID)
        )

    return bytes(output)


# =============================
# 物体（元のまま）
# =============================
def classify_color(bgr):
    b, g, r = bgr.astype(int)
    if max(r, g, b) < 40:
        return "black"
    if min(r, g, b) > 200:
        return "white"

    if r > g and r > b:
        if g > b: return "yellow"
        else: return "magenta"
    if g > r and g > b:
        if r > b: return "yellow"
        else: return "cyan"
    if b > r and b > g:
        if r > g: return "magenta"
        else: return "blue"

    return "red"


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
        cname = classify_color(bgr)
        cid = COLOR_IDS[cname]
        r, g, b = (bgr[::-1] / 255.0)

        output += struct.pack(
            "ffffffI",
            x, y, z,
            float(r), float(g), float(b),
            int(cid)
        )

    return bytes(output)


# =============================
# API
# =============================
@app.post("/pointcloud")
async def pointcloud(file: UploadFile = File(...)):
    image_bytes = await file.read()

    # 人間（468点）が検出できたらこちら
    human = get_human_points(image_bytes)
    if human is not None:
        return Response(content=human, media_type="application/octet-stream")

    # 人間でなければ物体分類
    obj = get_object_points(image_bytes)
    return Response(content=obj, media_type="application/octet-stream")
