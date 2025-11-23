using System;
using System.Collections;
using Common;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WebCamController : MonoBehaviour
{
    [Header("画面上の設定")]
    public RawImage displayImage; // カメラ映像を映す場所（RawImage）

    WebCamTexture webCamTexture;
    WebCamDevice[] devices;
    int currentCameraIndex = 0; // 現在のカメラ

    IEnumerator Start()
    {
        // 1. ユーザーにカメラ許可を求める
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);

        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            // 2. カメラデバイスを探す
            devices = WebCamTexture.devices;
            if (devices.Length == 0)
            {
                Debug.LogError("カメラが見つかりません");
                yield break;
            }

            // 3. 最初に背面カメラを選択
            for (int i = 0; i < devices.Length; i++)
            {
                if (!devices[i].isFrontFacing)
                {
                    currentCameraIndex = i;
                    break;
                }
            }

            StartCamera(currentCameraIndex);
        }
        else
        {
            Debug.LogError("カメラの許可がありません");
        }
    }

    void StartCamera(int index)
    {
        if (webCamTexture != null)
        {
            webCamTexture.Stop();
        }

        webCamTexture = new WebCamTexture(devices[index].name, 1280, 720);
        displayImage.texture = webCamTexture;
        webCamTexture.Play();
    }

    // ボタンから呼び出す「次のカメラに切り替え」
    public void NextCamera()
    {
        if (devices == null || devices.Length == 0) return;

        currentCameraIndex = (currentCameraIndex + 1) % devices.Length;
        StartCamera(currentCameraIndex);
        Debug.Log("カメラを切り替えました: " + devices[currentCameraIndex].name);
    }

    public void OnClickShutter()
    {
        if (webCamTexture == null || !webCamTexture.isPlaying) return;

        Texture2D photo = new Texture2D(webCamTexture.width, webCamTexture.height);
        photo.SetPixels(webCamTexture.GetPixels());
        photo.Apply();

        Debug.Log("パシャッ！ 撮影しました");

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
