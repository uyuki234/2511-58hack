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
        vel.y += deltaTime;
        time += deltaTime;
        pos += vel * deltaTime;
    }
}
public class RotateBullet : Bullet
{
    private Vector2 centerPos;
    private float time;
    private float rot;

    public RotateBullet(Vector2 pos, Color color, Vector2 centerPos) : base(pos, color)
    {
        this.centerPos = centerPos;
        this.pos = pos;
        this.rot = Mathf.Atan2(pos.y - centerPos.y, pos.x - centerPos.x);
        this.time = (pos - centerPos).magnitude * 0.2f;
    }

    public override void Update(float deltaTime)
    {
        this.time += deltaTime;
        this.rot += deltaTime;
        this.pos = new Vector2(Mathf.Cos(rot), Mathf.Sin(rot)) * time;
    }
}
public class GravityBullet : Bullet
{
    Vector2 vel;
    bool isExploded = false;
    public GravityBullet(Vector2 pos, Color color) : base(pos, color)
    {
    }

    public override void Update(float deltaTime)
    {
        if (!isExploded)
            this.vel.y -= deltaTime * 5f;
        this.pos += this.vel * deltaTime;
        if (this.pos.y < -8 && !isExploded)
        {
            isExploded = true;
            this.vel = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * 15f;
        }
    }
}
public class ChaseBullet : Bullet
{
    Vector2 vel;
    bool isExploded = false;
    public ChaseBullet(Vector2 pos, Color color) : base(pos, color)
    {
    }

    public override void Update(float deltaTime)
    {
        this.vel += ((Vector2)MainGameManager.Instance._player.transform.position - this.pos).normalized * deltaTime * 10f;
        this.pos += this.vel * deltaTime;
    }
}
public class SnakeBullet : Bullet
{
    Vector2 vel;
    bool isExploded = false;
    float time = 0f;
    float defaultY;
    float defaultX;
    public SnakeBullet(Vector2 pos, Color color) : base(pos, color)
    {
        vel.y = -10f;
        defaultY = pos.y;
        defaultX = pos.x;
    }

    public override void Update(float deltaTime)
    {
        time += deltaTime;
        this.pos.x = defaultX + Mathf.Sin(time * (-2.5f) + defaultY) * 1f;
        this.pos += this.vel * deltaTime;
    }
}