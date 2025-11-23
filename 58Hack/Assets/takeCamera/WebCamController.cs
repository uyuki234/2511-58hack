using System;
using System.Collections;
using Common;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // UIを扱うために必要
using System.Runtime.InteropServices;

public class WebCamController : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void GetBackCameraStream();
    [Header("画面上の設定")]
    public RawImage displayImage; // カメラ映像を映す場所（RawImage）

    WebCamTexture webCamTexture;

    // ゲーム開始時に自動で動く
    IEnumerator Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
    // WebGL（iPhone Safari）では JS 側で背面カメラを取得
    GetBackCameraStream();
    yield break;  // WebCamTexture は使わない
#endif

        // ↓ ここからはネイティブアプリ用（iOSアプリ/Android）
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);

        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            WebCamDevice[] devices = WebCamTexture.devices;
            if (devices.Length == 0)
            {
                Debug.LogError("カメラが見つかりません");
                yield break;
            }

            string cameraName = devices[0].name;
            for (int i = 0; i < devices.Length; i++)
            {
                if (!devices[i].isFrontFacing)
                {
                    cameraName = devices[i].name;
                    break;
                }
            }

            webCamTexture = new WebCamTexture(cameraName, 1280, 720);
            displayImage.texture = webCamTexture;
            webCamTexture.Play();
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

        //DataConnector connector = new DataConnector();
        //StartCoroutine(((IDataReceiver)connector).GetData(photo,OnPoints));
        MainGameManager.targetTexture = photo;
        SceneManager.LoadScene("MainGame");
    }

    public void OnPoints(PicturePoints points)
    {
        Debug.Log("Get Data is Success");
        if (points == null)
        {
            Debug.Log("Points is null . You should check server and Texture2D");
            return;
        }
        Point[] ps = points.GetPoints();
        for (int i = 0; i < Math.Min(ps.Length, 5); i++)
        {
            Debug.Log($"position:{ps[i].pos},color:{ps[i].color}");
        }

        Debug.Log($"{points.GetResolution()}");

    }
}