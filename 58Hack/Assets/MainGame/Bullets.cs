using UnityEngine;

public class StandardBullet : Bullet
{
    private Vector2 vel;
    private float time;
    public StandardBullet(Vector2 pos, Color color, Vector2 vel) : base(pos, color)
    {
        this.vel = vel;
    }
    public override void Update(float deltaTime)
    {
        //vel += this.pos.normalized * deltaTime * 2;
        //vel += new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        time += deltaTime;
        pos += vel * deltaTime;
    }
}