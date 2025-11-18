using System;
using System.Collections;
using UnityEngine;
using Unity.Collections;
using UnityEngine.Networking;

public class PostImage : MonoBehaviour
{
	// 画像タイプ情報
	public struct ImageType
	{
		public string extension;
		public string fileName;
	}

    private string url = "http://127.0.0.1:8000/pointcloud";

    // 呼び出し方: StartCoroutine(GetNativeArrayFromPython(..., (result) => { ... }));
    public IEnumerator GetNativeArrayFromPython(byte[] imagedata, ImageType imagetype, Action<NativeArray<Vector2>> onComplete)
    {
        // 1. フォームデータの作成 (curl --form 'file=@...' に対応)
        WWWForm form = new WWWForm();

        // FastAPIはファイルの中身を見るので、mimeTypeは "image/png" 等、適当でも動くことが多いです
        string mimeType = "image/" + imagetype.extension;
        form.AddBinaryData("file", imagedata, imagetype.fileName, mimeType);

        // 2. リクエストの作成
        using (UnityWebRequest www = UnityWebRequest.Post(this.url, form))
        {
            www.downloadHandler = new DownloadHandlerBuffer();

            // 3. 送信して待機
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {www.error}");
                Debug.LogError($"Server Response: {www.downloadHandler.text}"); // サーバーのエラー詳細を見る
            }
            else
            {
                // 4. 成功！データの取り出し
                byte[] responseBytes = www.downloadHandler.data;
                Debug.Log($"[INFO] Received {responseBytes.Length} bytes.");

                if (responseBytes.Length == 0)
                {
                    Debug.LogWarning("[INFO] No data received.");
                    yield break;
                }

                int pointCount = responseBytes.Length / 8;
                
                // NativeArrayを確保 (Allocator.Persistent 推奨: 呼び出し元でDisposeするため)
                NativeArray<Vector2> pointCloud = 
                    new NativeArray<Vector2>(pointCount, Allocator.Persistent);

                // バイト列を float 配列に変換（各点は float x, float y の順で格納されている想定）
                if (responseBytes.Length % 8 != 0)
                {
                    Debug.LogWarning("[WARN] Response byte length is not a multiple of 8; truncating.");
                }

                int floatCount = pointCount * 2;
                float[] floats = new float[floatCount];
                // Buffer.BlockCopy を使って byte[] を float[] にコピー（サーバーとクライアントでエンディアンが一致している前提）
                Buffer.BlockCopy(responseBytes, 0, floats, 0, Math.Min(responseBytes.Length, floatCount * sizeof(float)));

                // float 配列から Vector2 配列へ変換
                Vector2[] vectors = new Vector2[pointCount];
                for (int i = 0; i < pointCount; i++)
                {
                    vectors[i] = new Vector2(floats[i * 2], floats[i * 2 + 1]);
                }

                // NativeArray にコピー
                pointCloud.CopyFrom(vectors);

                Debug.Log($"[INFO] Parsed {pointCloud.Length} points.");

                // 5. コールバックで呼び出し元にデータを渡す
                onComplete?.Invoke(pointCloud);
            }
        }
    }

    // jpgData をサーバーに送り、NativeArray<Vector2> をコールバックで返す（コルーチン）
    // 実運用: UnityWebRequest.Post などで jpgData を送信し、レスポンスを解析して NativeArray を生成してください。
    public IEnumerator GetNativeArrayFromPythonMock(byte[] jpgData, ImageType type, Action<NativeArray<Vector2>> callback)
    {
        // デバッグ用の短い待ち時間（実通信の代わり）
        yield return new WaitForSeconds(0.2f);

        // ダミー点群を生成（Persistent で返すのは呼び出し側が Dispose するため）
        int count = 128;
        var points = new NativeArray<Vector2>(count, Allocator.Persistent);
        for (int i = 0; i < count; i++)
        {
            points[i] = new Vector2(UnityEngine.Random.value, UnityEngine.Random.value);
        }

        // コールバックで返す
        callback?.Invoke(points);
    }
}