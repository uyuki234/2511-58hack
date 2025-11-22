using Common;
using UnityEngine;
using Zenject;

public class MainGameManager : MonoBehaviour
{
    [SerializeField] BulletManager _bulletManager;
    [SerializeField] GameObject _player;
    [Inject] private IDataReceiver _dataReceiver;
    public static Texture2D targetTexture;
    void Start()
    {
        _bulletManager.Init();
        print(targetTexture);
        StartCoroutine(_dataReceiver.GetData(targetTexture, (x) => Spawn(x)));
    }
    void Spawn(PicturePoints picturePoints)
    {
        Debug.Log("yeah!");
        foreach (var point in picturePoints.GetPoints())
        {
            _bulletManager.AddBullet(new StandardBullet(new Vector2(point.pos.x - 0.5f, point.pos.y * -1f - 0.5f) * 20, point.color, new Vector2(point.pos.x - 0.5f, point.pos.y - 0.5f).normalized));
        }
    }
    void Update()
    {
        _bulletManager.Update(Time.deltaTime);
        UpdatePlayer();
    }
    Vector3 prevPos;
    bool dragging = false;
    void UpdatePlayer()
    {
        Vector3 currentPos;
        bool isDown = false;
        bool isUp = false;

        // --- タッチ入力 ---
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);

            currentPos = t.position;

            if (t.phase == TouchPhase.Began)
            {
                isDown = true;
            }
            else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
            {
                isUp = true;
            }
            else
            {
                isDown = true; // 触っている間
            }
        }
        // --- マウス入力 ---
        else
        {
            currentPos = Input.mousePosition;
            isDown = Input.GetMouseButton(0);
            isUp = Input.GetMouseButtonUp(0);
        }

        // 押した瞬間
        if (!dragging && isDown)
        {
            dragging = true;
            prevPos = currentPos;
        }

        // ドラッグ中
        if (dragging && isDown)
        {
            Vector3 delta = currentPos - prevPos;

            // 画面上の変化量をワールドに適用
            // ※必要に応じてスケール調整
            _player.transform.position += new Vector3(delta.x, delta.y, 0) * 0.01f;

            prevPos = currentPos;
        }

        // 離したら終了
        if (isUp)
        {
            dragging = false;
        }

        if (_bulletManager.GetIsPointInBullet(_player.transform.position))
        {
            print("hit!");
        }
    }
}
