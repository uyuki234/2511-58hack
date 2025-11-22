using System.Collections;
using UnityEngine;
using UnityEngine.UI; // UIを扱うために必要

public class WebCamController : MonoBehaviour
{
    [Header("画面上の設定")]
    public RawImage displayImage; // カメラ映像を映す場所（RawImage）

    WebCamTexture webCamTexture;

    // ゲーム開始時に自動で動く
    IEnumerator Start()
    {
        // 1. ユーザーにカメラ許可を求める
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);

        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            // 2. カメラデバイスを探す
            WebCamDevice[] devices = WebCamTexture.devices;
            if (devices.Length == 0)
            {
                Debug.LogError("カメラが見つかりません");
                yield break;
            }

            // 3. 使うカメラを決める（スマホなら背面、PCならデフォルト）
            string cameraName = devices[0].name;
            for (int i = 0; i < devices.Length; i++)
            {
                if (!devices[i].isFrontFacing) // 背面カメラを優先
                {
                    cameraName = devices[i].name;
                    break;
                }
            }

            // 4. カメラを起動してRawImageにセットする
            // ※サイズは仮で1280x720にしていますが、スマホに合わせて調整されます
            webCamTexture = new WebCamTexture(cameraName, 1280, 720);
            displayImage.texture = webCamTexture;
            webCamTexture.Play(); // 撮影開始！
        }
        else
        {
            Debug.LogError("カメラの許可がありません");
        }
    }

    // ボタンが押されたら動く関数
    public void OnClickShutter()
    {
        if (webCamTexture == null || !webCamTexture.isPlaying) return;

        // 撮影（テクスチャを切り出す）
        Texture2D photo = new Texture2D(webCamTexture.width, webCamTexture.height);
        photo.SetPixels(webCamTexture.GetPixels());
        photo.Apply();

        Debug.Log("パシャッ！ 撮影しました");

        // ★ここに後で「APIへ送信する処理」を追加します
        // SendPhotoToApi(photo); みたいな感じです
    }
}