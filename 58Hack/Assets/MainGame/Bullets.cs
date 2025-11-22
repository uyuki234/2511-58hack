using UnityEngine;

public class StandardBullet : Bullet
{
    private Vector2 vel;
    private float time;
    public StandardBullet(Vector2 pos, Color color) : base(pos, color)
    {
    }
    public override void Update(float deltaTime)
    {
        color += new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)) * deltaTime;
        vel += this.pos.normalized * deltaTime * 2;
        vel += new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        time += deltaTime;
        pos += vel * deltaTime;
    }
}