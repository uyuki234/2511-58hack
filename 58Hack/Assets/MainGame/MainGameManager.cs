using Common;
using UnityEngine;
using Zenject;

public class MainGameManager : MonoBehaviour
{
    [SerializeField] BulletManager _bulletManager;
    [SerializeField] GameObject _player;
    [Inject] private IDataReceiver _dataReceiver;
    public static Texture2D targetTexture;
    private PicturePoints _picturePoints;
    Vector2 average;
    void Start()
    {
        _bulletManager.Init();
        print(targetTexture);
        StartCoroutine(_dataReceiver.GetData(targetTexture, (x) => Spawn(x)));
    }
    void Spawn(PicturePoints picturePoints)
    {
        _picturePoints = picturePoints;
        Vector2 sum = Vector2.zero;
        foreach (var point in _picturePoints.GetPoints())
        {
            sum += new Vector2(point.pos.x - 0.5f, point.pos.y * (-1f) + 1f) * 20;
        }
        sum /= _picturePoints.GetPoints().Length;
        average = sum;
    }
    void Update()
    {
        if (Random.Range(0f, 1f) < 0.005f)
        {
            Vector2 randomOffset = new Vector2(Random.Range(-10f, 10f), Random.Range(-1f, 1f));
            foreach (var point in _picturePoints.GetPoints())
            {
                Vector2 thePos = new Vector2(point.pos.x - 0.5f, point.pos.y * (-1f) + 1f) * 20;
                Vector2 vel = (thePos - average) * 5f;
                _bulletManager.AddBullet(new StandardBullet(thePos + randomOffset, point.color, vel));
            }
        }
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
