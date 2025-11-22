using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BulletManager
{
    [SerializeField] private Mesh _mesh;
    [SerializeField] private Material _material;
    private List<Bullet> _bullets;
    BulletRenderer _bulletRenderer;
    public void Init()
    {
        _bullets = new List<Bullet>();
        _bulletRenderer = new BulletRenderer(_mesh, _material);
    }
    public void Update(float deltaTime)
    {
        UpdateBullets(deltaTime);
        _bulletRenderer.Render(_bullets);
    }
    public void AddBullet(Bullet bullet)
    {
        bullet.Init(() => _bullets.Remove(bullet));
        _bullets.Add(bullet);
    }
    private void UpdateBullets(float deltaTime)
    {
        for (int i = _bullets.Count - 1; i >= 0; i--)
        {
            _bullets[i].Update(deltaTime);
        }
    }
}
internal class BulletRenderer
{
    private const int MeshCount = 1023;
    private Matrix4x4[] _matrices;
    private MaterialPropertyBlock _propertyBlock;
    private Mesh _mesh;
    private Material _material;
    public BulletRenderer(Mesh mesh, Material material)
    {
        this._mesh = mesh;
        this._material = material;
    }
    public void Render(List<Bullet> bullets)
    {
        _matrices = new Matrix4x4[bullets.Count];
        _propertyBlock = new MaterialPropertyBlock();

        var colors = new Vector4[bullets.Count];
        for (int i = 0; i < bullets.Count; i++)
        {
            var pos = new Vector3
            (
                bullets[i].pos.x,
                bullets[i].pos.y,
                0f
            );
            _matrices[i] = Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one);
            colors[i] = new Vector4(bullets[i].color.r, bullets[i].color.g, bullets[i].color.b, 1f);
        }
        _propertyBlock.SetVectorArray("_Color", colors);
        Graphics.DrawMeshInstanced(_mesh, 0, _material, _matrices, bullets.Count, _propertyBlock);
    }
}
public abstract class Bullet
{
    protected internal Vector2 pos;
    protected internal Color color;
    private Action _destroyer;
    public Bullet(Vector2 pos, Color color)
    {
        this.pos = pos;
        this.color = color;
    }
    internal void Init(Action destroyer)
    {
        this._destroyer = destroyer;
    }
    public abstract void Update(float deltaTime);
    protected internal void Destroy()
    {
        this._destroyer();
    }
}
