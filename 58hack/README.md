unityで開いてね
- unity 3dエンジン
# API取得関数
## MainController.start()
- モックデータを送信。
- `/mock/`に任意の画像を入れてこの関数の中身を差し替えると点群が取得可能
## MainController.OnPointCloudReceived()
- 点群をpythonサーバーから受け取った後の処理を書く。

# API TEST
## モックデータで
## MainController.csから点群を取得してcsコード上に出力することに成功

`/58hack/Assets/mock` の中に任意の画像ファイルを入れてstart関数の中身を書き換えたら可能

## 今後の開発：
- 点群を表示させるhlslを書く
- 写真を撮ってpythonに送れるようにする
  - ui作成
  - 送信機能作成
`class MainController` 内にて
  ```cs
//...
class MainController : MonoBehaviour {
    void Start (){
        this.MockTest ("{fileName}") ;
        this.SendImage () ;
    }
    // ... 
    void OnPointCloudReceived(NativeArray<Vector2> points) {
        // 点群が取得可能
        for(int i = 0 ; i < points.Length ; i++){
            Debug.Log(points[i]);
        }
    }
    //...
}
  ```