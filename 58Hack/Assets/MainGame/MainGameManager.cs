using UnityEngine;

public class MainGameManager : MonoBehaviour
{
    [SerializeField] BulletManager _bulletManager;
    void Start()
    {
        _bulletManager.Init();
    }
    void Update()
    {
        _bulletManager.AddBullet(new StandardBullet(new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)), new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f))));
        _bulletManager.Update(Time.deltaTime);
    }
}
