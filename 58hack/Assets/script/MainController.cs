using System.Collections;
using UnityEngine;
using Unity.Collections;
using System.IO;
using UnityEngine.Networking;
using static PostImage;

public class MainController : MonoBehaviour
{
    [SerializeField] PostImage postImageScript;
    [SerializeField] Texture2D myTexture; // テスト用画像

    void Start()
    {
        // まずプロジェクト内の Assets/mock/kim.png を探す
        string filePath = Path.Combine(Application.dataPath, "mock", "kim.png");
        if (File.Exists(filePath))
        {
            LoadTextureFromFile(filePath);
        }
        else
        {
            // 見つからなければ Resources フォルダを試す（Assets/Resources/mock/kim.png として配置）
            Texture2D resTex = Resources.Load<Texture2D>("mock/kim");
            if (resTex != null)
            {
                myTexture = resTex;
                Debug.Log("Loaded Texture from Resources: mock/kim");
            }
            else
            {
                Debug.LogError("画像が見つかりませんでした。期待される配置場所:\n" +
                    "1) Assets/mock/kim.png  または\n" +
                    "2) Assets/Resources/mock/kim.png (Resources.Load 用)\n" +
                    "現在の検索パス: " + filePath);
            }
        }

        // Inspector 未割当ならシーン内から自動検索して割り当てを試みる
        if (postImageScript == null)
        {
            // Use the newer API to find a PostImage instance in the scene.
            postImageScript = Object.FindFirstObjectByType<PostImage>();
            Debug.Log("postImageScript auto-assign: " + (postImageScript == null ? "null (PostImage not found in scene)" : "assigned via FindFirstObjectByType"));
        }
        else
        {
            Debug.Log("postImageScript is assigned (Inspector)");
        }

        // SendImage 呼び出し（myTexture が設定されている場合のみ）
        Debug.Log("postImageScript is " + (postImageScript == null ? "null" : "assigned"));
        this.SendImage();
    }

    void SendImage()
    {
        if (myTexture == null)
        {
            Debug.LogError("SendImage aborted: myTexture is null");
            return;
        }

        if (postImageScript == null)
        {
            Debug.LogError("SendImage aborted: postImageScript is null. Assign PostImage in the Inspector or add a PostImage to the scene.");
            return;
        }

        // 画像をバイナリ化
        byte[] jpgData = myTexture.EncodeToJPG();
        
        ImageType type = new ImageType { extension = "jpg", fileName = "test.jpg" };

        // コルーチンを開始（安全に呼び出す）
        try
        {
            StartCoroutine(postImageScript.GetNativeArrayFromPython(jpgData, type, OnPointCloudReceived));
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to start coroutine or call GetNativeArrayFromPython: " + ex);
        }
    }

    // データが返ってきたら実行される関数
    /// <summary>
    /// @akiraへ
    /// ここで写真からVector2を取得した後の処理をかける。
    /// </summary>
    /// <param name="points"></param>
    void OnPointCloudReceived(NativeArray<Vector2> points)
    {
        Debug.Log("点群を受け取りました！ 点の数: " + points.Length);

        for(int i = 0 ; i < points.Length ; i++){
            Debug.Log(points[i]);
        }

        // --- ここでHLSLなどに渡す処理を書く ---
        
        // 重要: NativeArray (Persistent) は使い終わったら必ず解放する！
        // すぐに使わないなら、保持しておいてOnDestroyなどで解放してください。
        points.Dispose(); 
    }


    // Resources フォルダから読み込む
    public void LoadTextureFromResources(string resourcePath)
    {
        Texture2D tex = Resources.Load<Texture2D>(resourcePath);
        if (tex != null)
        {
            myTexture = tex;
            Debug.Log("Loaded Texture from Resources: " + resourcePath);
        }
        else
        {
            Debug.LogError("Resources からテクスチャが見つかりません: " + resourcePath);
        }
    }

    // ローカルファイルから読み込む（任意の拡張子、Read/Write の設定不要）
    public void LoadTextureFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("ファイルが存在しません: " + filePath);
            return;
        }

        try
        {
            byte[] bytes = File.ReadAllBytes(filePath);
            Texture2D tex = new Texture2D(2, 2);
            if (tex.LoadImage(bytes)) // true なら画像として読み込めた
            {
                myTexture = tex;
                Debug.Log("Loaded Texture from file: " + filePath);
            }
            else
            {
                Debug.LogError("LoadImage に失敗しました: " + filePath);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("ファイル読み込みエラー: " + ex.Message);
        }
    }

    // URL から読み込む（非同期、コルーチンで使用）
    public IEnumerator LoadTextureFromURL(string url)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (uwr.result != UnityWebRequest.Result.Success)
#else
            if (uwr.isNetworkError || uwr.isHttpError)
#endif
            {
                Debug.LogError("画像ダウンロード失敗: " + uwr.error);
            }
            else
            {
                Texture2D tex = DownloadHandlerTexture.GetContent(uwr);
                if (tex != null)
                {
                    myTexture = tex;
                    Debug.Log("Loaded Texture from URL: " + url);
                }
            }
        }
    }
}