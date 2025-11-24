using System.Collections.Generic;
using Common;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Zenject;

public class MainGameManager : MonoBehaviour
{
    [SerializeField] BulletManager _bulletManager; [SerializeField] public GameObject _player;
    [SerializeField] HeartsUI _heartsUI;
    [SerializeField] TextMeshProUGUI tmp;
    [SerializeField] TextMeshProUGUI debug;
    [SerializeField] AudioClip ac;
    [SerializeField] AudioSource aS;
    int faces = 0;
    int heart = 3;
    [Inject] private IDataReceiver _dataReceiver;
    public static Texture2D targetTexture;
    private PicturePoints _picturePoints;
    private float hitCooldownCounter;
    Vector2 average;
    public static MainGameManager Instance;
    public static int lastFaceCount;

    float coolTime = 3f;
    float time = 0f;

    List<System.Action<Vector2, PicturePoints>> bulletPatternes;
    void Start()
    {
        MainGameManager.Instance = this;
        _bulletManager.Init();
        print(targetTexture);
        debug.text = targetTexture.width + ":" + targetTexture.height;
        StartCoroutine(_dataReceiver.GetData(targetTexture, (x) => Spawn(x)));
        heart = 3;

        bulletPatternes = new List<System.Action<Vector2, PicturePoints>>()
    {
      (randomOffset, _picturePoints) =>
      {
        foreach (var point in _picturePoints.GetPoints())
            {
                Vector2 thePos = new Vector2(point.pos.x - 0.5f, point.pos.y * (-1f) + 1f) * 20;
                Vector2 vel = (thePos - average) * 5f;
                _bulletManager.AddBullet(new ChaseBullet(thePos + randomOffset, point.color));
            }
      },
      (randomOffset, _picturePoints) =>
      {
        foreach (var point in _picturePoints.GetPoints())
            {
                Vector2 thePos = new Vector2(point.pos.x - 0.5f, point.pos.y * (-1f) + 1f) * 20;
                Vector2 vel = (thePos - average) * 5f;
                _bulletManager.AddBullet(new GravityBullet(thePos + randomOffset, point.color));
            }
      },
      (randomOffset, _picturePoints) =>
      {
        foreach (var point in _picturePoints.GetPoints())
            {
                Vector2 thePos = new Vector2(point.pos.x - 0.5f, point.pos.y * (-1f) + 1f) * 20;
                Vector2 vel = (thePos - average) * 5f;
                _bulletManager.AddBullet(new SnakeBullet(thePos + randomOffset, point.color));
            }
      },
      (randomOffset, _picturePoints) =>
      {
        foreach (var point in _picturePoints.GetPoints())
            {
                Vector2 thePos = new Vector2(point.pos.x - 0.5f, point.pos.y * (-1f) + 1f) * 20;
                Vector2 vel = (thePos - average) * 5f;
                _bulletManager.AddBullet(new RotateBullet(thePos + randomOffset, point.color, randomOffset));
            }
      },
      (randomOffset, _picturePoints) =>
      {
        foreach (var point in _picturePoints.GetPoints())
            {
                Vector2 thePos = new Vector2(point.pos.x - 0.5f, point.pos.y * (-1f) + 1f) * 20;
                Vector2 vel = (thePos - average) * 5f;
                _bulletManager.AddBullet(new StandardBullet(thePos + randomOffset, point.color, vel));
            }
      }
    };

    }
    void Spawn(PicturePoints picturePoints)
    {
        Debug.Log(targetTexture.width + ":" + targetTexture.height);
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
        coolTime -= Time.deltaTime;
        if (coolTime < 0f)
        {
            Vector2 randomOffset = new Vector2(Random.Range(-10f, 10f), Random.Range(-1f, 1f) + 5f);
            /*foreach (var point in _picturePoints.GetPoints())
            {
                Vector2 thePos = new Vector2(point.pos.x - 0.5f, point.pos.y * (-1f) + 1f) * 20;
                Vector2 vel = (thePos - average) * 5f;
                _bulletManager.AddBullet(new StandardBullet(thePos + randomOffset, point.color, vel));
            }*/
            bulletPatternes[Random.Range(0, bulletPatternes.Count)](randomOffset, _picturePoints);
            faces += 1;
            tmp.text = $"{faces} faces!";
            coolTime = 5 - Mathf.Pow(time, 0.3f);
        }
        _bulletManager.Update(Time.deltaTime);
        UpdatePlayer();
        time += Time.deltaTime;
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
            float z = Camera.main.nearClipPlane; // 0 でもOK

            Vector3 worldDelta =
                Camera.main.ScreenToWorldPoint(Input.mousePosition + new Vector3(0, 0, z)) -
                Camera.main.ScreenToWorldPoint(prevPos + new Vector3(0, 0, z));

            // 画面上の変化量をワールドに適用
            // ※必要に応じてスケール調整
            _player.transform.position += worldDelta;

            prevPos = currentPos;
        }

        // 離したら終了
        if (isUp)
        {
            dragging = false;
        }

        hitCooldownCounter -= Time.deltaTime;
        if (hitCooldownCounter < 0f && _bulletManager.GetIsPointInBullet(_player.transform.position))
        {
            Hit();
            hitCooldownCounter = 3f;
        }
    }
    System.Collections.IEnumerator ShakeCamera(Transform cam, float duration, float magnitude)
    {
        Vector3 originalPos = cam.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            cam.localPosition = originalPos + (Vector3)Random.insideUnitCircle * magnitude;
            elapsed += Time.deltaTime;
            yield return null;
        }

        cam.localPosition = originalPos;
    }
    void Hit()
    {
        if (heart > 0)
        {
            _heartsUI.RemoveHeart();
            heart -= 1;
            aS.PlayOneShot(ac);
            StartCoroutine(ShakeCamera(Camera.main.transform, 0.5f, 0.5f));
            /*foreach (var bullet in _bulletManager._bullets)
            {
                bullet.Destroy();
            }*/
        }
        if (heart <= 0)
        {
            lastFaceCount = faces;
            SceneManager.LoadScene("Result");
        }
    }
}
