using System;
using System.Collections;
using System.IO;
using Common;
using UnityEngine;
using UnityEngine.Networking;

public class DataConnector : IDataReceiver
{
    [SerializeField] private string URI = "http://127.0.0.1:8000/pointcloud";
    [SerializeField] private string imageFileName = "Assets/SamplePicture/generated-image.png";

    IEnumerator IDataReceiver.GetData(Action<PicturePoints> callback)
    {
        string path = ResolveImagePath(imageFileName);
        Debug.Log($"[DataConnector] Resolved path: {path ?? "NULL"}");
        return FetchData(path, callback);
    }

    public IEnumerator FetchData(string imagePath, Action<PicturePoints> callback)
    {
        if (string.IsNullOrEmpty(imagePath))
        {
            Debug.LogWarning("[DataConnector] Image path null/empty.");
            callback?.Invoke(EmptyPoints());
            yield break;
        }
        if (!File.Exists(imagePath))
        {
            Debug.LogWarning("[DataConnector] File not found: " + imagePath);
            callback?.Invoke(EmptyPoints());
            yield break;
        }

        byte[] fileBytes;
        fileBytes = File.ReadAllBytes(imagePath);
        Debug.Log($"[DataConnector] Read bytes: {fileBytes.Length}");

        var form = new WWWForm();
        form.AddBinaryData("file", fileBytes, Path.GetFileName(imagePath));
        using (var req = UnityWebRequest.Post(URI, form))
        {
            req.timeout = 10;
            Debug.Log("[DataConnector] Sending request: " + URI);
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
            pts = ParsePointCloud(data);
            Debug.Log($"[DataConnector] Parsed points: {pts.GetPoints().Length}");
            callback?.Invoke(pts);
        }
    }

    private static PicturePoints ParsePointCloud(byte[] bytes)
    {
        int floatCount = bytes.Length / 4;
        if (floatCount == 0 || floatCount % 6 != 0)
            throw new Exception("Invalid float count: " + floatCount);

        float[] floats = new float[floatCount];
        Buffer.BlockCopy(bytes, 0, floats, 0, bytes.Length);

        int pointCount = floatCount / 6;
        var points = new Point[pointCount];

        for (int i = 0; i < pointCount; i++)
        {
            int idx = i * 6;
            float x = Clamp01(floats[idx]);
            float y = Clamp01(floats[idx + 1]);
            // z is at floats[idx + 2], but we ignore it.
            float r = Clamp01(floats[idx + 3]);
            float g = Clamp01(floats[idx + 4]);
            float b = Clamp01(floats[idx + 5]);
            points[i] = new Point
            {
                pos = new Vector2(x, y),
                color = new Color(r, g, b, 1f)
            };
        }
        return new PicturePoints(points, new Vector2Int(1, 1)); // 解像度仮
    }

    private static PicturePoints EmptyPoints() =>
        new PicturePoints(Array.Empty<Point>(), new Vector2Int(0, 0));

    private static float Clamp01(float v) => v < 0f ? 0f : (v > 1f ? 1f : v);

    private static string ResolveImagePath(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        if (Path.IsPathRooted(name) && File.Exists(name)) return name;
        if (File.Exists(name)) return Path.GetFullPath(name);

        string sa = Path.Combine(Application.streamingAssetsPath, name);
        if (File.Exists(sa)) return sa;

        string pd = Path.Combine(Application.persistentDataPath, name);
        if (File.Exists(pd)) return pd;

#if UNITY_EDITOR
        string assets = Path.Combine(Application.dataPath, name);
        if (File.Exists(assets)) return assets;
#endif
        return null;
    }
}
