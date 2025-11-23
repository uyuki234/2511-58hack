using System;
using System.Collections;
using System.IO;
using Common;
using UnityEngine;
using UnityEngine.Networking;

public class DataConnector : IDataReceiver
{
    [SerializeField] private string URI = "http://127.0.0.1:8000/pointcloud"; // ← https を http に

    IEnumerator IDataReceiver.GetData(Texture2D img,Action<PicturePoints> callback)
    {
        if (img == null)
        {
            Debug.LogWarning("[DataConnector] Texture null.");
            callback?.Invoke(EmptyPoints());
            yield break;
        }

        byte[] fileBytes = null;
        // try
        // {
            fileBytes = img.EncodeToJPG(); // 必要なら EncodeToJPG へ変更可
        // }
        // catch (Exception e)
        // {
        //     Debug.LogWarning("[DataConnector] Encode failed: " + e.Message);
        //     callback?.Invoke(EmptyPoints());
        //     yield break;
        // }

        if (fileBytes == null || fileBytes.Length == 0)
        {
            Debug.LogWarning("[DataConnector] Encoded bytes empty.");
            callback?.Invoke(EmptyPoints());
            yield break;
        }

        yield return PostBytes(fileBytes, "uploaded.jpg", callback);
    }

    public IEnumerator PostBytes(byte[] fileBytes, string fileName, Action<PicturePoints> callback)
    {
        if (fileBytes == null || fileBytes.Length == 0)
        {
            Debug.LogWarning("[DataConnector] No bytes to upload.");
            callback?.Invoke(EmptyPoints());
            yield break;
        }

        var form = new WWWForm();
        form.AddBinaryData("file", fileBytes, fileName);
        using (var req = UnityWebRequest.Post(URI, form))
        {
            req.timeout = 10;
            // もし https + 自己署名証明書を許可したい場合は下記を追加（本番禁止）
            // req.certificateHandler = new DevIgnoreCert();
            Debug.Log("[DataConnector] Sending request bytes: " + URI);
            yield return req.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
            if (req.result != UnityWebRequest.Result.Success)
#else
            if (req.isHttpError || req.isNetworkError)
#endif
            {
                Debug.LogWarning("[DataConnector] Request failed: " + req.error);
                callback?.Invoke(EmptyPoints());
                yield break;
            }

            byte[] data = req.downloadHandler.data;
            Debug.Log($"[DataConnector] Received bytes: {(data == null ? 0 : data.Length)}");
            if (data == null || data.Length == 0)
            {
                Debug.LogWarning("[DataConnector] Empty response.");
                callback?.Invoke(EmptyPoints());
                yield break;
            }

            PicturePoints pts;
            // try
            // {
                pts = ParsePointCloud(data);
            // }
            // catch (Exception ex)
            // {
            //     Debug.LogWarning("[DataConnector] Parse error: " + ex.Message);
            //     callback?.Invoke(EmptyPoints());
            //     yield break;
            // }
            Debug.Log($"[DataConnector] Parsed points: {pts.GetPoints().Length}");
            callback?.Invoke(pts);
        }
    }

    private static PicturePoints ParsePointCloud(byte[] bytes)
    {
        const int bytesPerPoint = 28; // 6 floats + 1 uint32
        if (bytes.Length == 0 || bytes.Length % bytesPerPoint != 0)
            throw new Exception("Invalid byte length: " + bytes.Length);

        int pointCount = bytes.Length / bytesPerPoint;

        var xs = new float[pointCount];
        var ys = new float[pointCount];
        var rs = new float[pointCount];
        var gs = new float[pointCount];
        var bs = new float[pointCount];
        var ids = new int[pointCount];

        int offset = 0;
        bool needNormalize = false;
        for (int i = 0; i < pointCount; i++)
        {
            float x = ReadLEFloat(bytes, ref offset);
            float y = ReadLEFloat(bytes, ref offset);
            float z = ReadLEFloat(bytes, ref offset); // z は未使用

            float r = ReadLEFloat(bytes, ref offset);
            float g = ReadLEFloat(bytes, ref offset);
            float b = ReadLEFloat(bytes, ref offset);

            uint rawId = ReadLEUInt(bytes, ref offset);

            xs[i] = x;
            ys[i] = y;
            rs[i] = r;
            gs[i] = g;
            bs[i] = b;
            ids[i] = (int)rawId;

            if (x > 1f || y > 1f) needNormalize = true;
        }

        if (needNormalize)
        {
            float maxX = 0f, maxY = 0f;
            for (int i = 0; i < pointCount; i++)
            {
                if (xs[i] > maxX) maxX = xs[i];
                if (ys[i] > maxY) maxY = ys[i];
            }
            if (maxX <= 0f) maxX = 1f;
            if (maxY <= 0f) maxY = 1f;
            float invX = 1f / maxX;
            float invY = 1f / maxY;
            for (int i = 0; i < pointCount; i++)
            {
                xs[i] *= invX;
                ys[i] *= invY;
            }
        }

        var points = new Point[pointCount];
        for (int i = 0; i < pointCount; i++)
        {
            points[i] = new Point
            {
                pos = new Vector2(xs[i], ys[i]),          // 0〜1 に統一
                color = new Color(rs[i], gs[i], bs[i], 1f),
                id = ids[i]
            };
        }

        return new PicturePoints(points, new Vector2Int(1, 1));
    }

    private static float ReadLEFloat(byte[] src, ref int offset)
    {
        float v = BitConverter.ToSingle(src, offset);
        offset += 4;
        return v;
    }

    private static uint ReadLEUInt(byte[] src, ref int offset)
    {
        uint v = BitConverter.ToUInt32(src, offset);
        offset += 4;
        return v;
    }

    private static PicturePoints EmptyPoints() =>
        new PicturePoints(Array.Empty<Point>(), new Vector2Int(0, 0));

    private static float Clamp01(float v) => v < 0f ? 0f : (v > 1f ? 1f : v);

}
