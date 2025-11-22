using UnityEngine;
using Common;

public class APITest : MonoBehaviour
{
    // テスト用なんで容赦なく消してください。
    private DataConnector connector = new DataConnector();

    void Start()
    {
        // コルーチン開始
        StartCoroutine(((IDataReceiver)connector).GetData(OnPoints));
    }

    private void OnPoints(PicturePoints points)
    {
        if (points == null)
        {
            Debug.LogWarning("PicturePoints is null.");
            return;
        }

        var arr = points.GetPoints();
        // ここがtrueになってしまう
        if(arr.Length == 0)
        {
            Debug.Log("Error");
            return ;
        }
        Debug.Log($"Received points count = {arr.Length}");

        // 先頭5件だけ確認
        int preview = Mathf.Min(5, arr.Length);
        for (int i = 0; i < preview; i++)
        {
            var p = arr[i];
            Debug.Log($"[{i}] pos=({p.pos.x:F3},{p.pos.y:F3}) color=({p.color.r:F3},{p.color.g:F3},{p.color.b:F3})");
        }
    }
}
