using UnityEngine;

public class MysteryAlien : Alien
{
    const int MOVE_DELAY = 2;
    const int HEIGHT = 210;

    byte ticksToMove;
    bool isGoingRight;
    bool isOutOfRange;

    private void Awake()
    {
        Size = new Vector2Int(10, 8);
        SpriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
    }

    internal void Spawn()
    {
        this.gameObject.SetActive(true);
        isGoingRight = Random.Range(0, 2) == 1;
        Position = new Vector2Int(isGoingRight ? 0 : GameplayController.WIDTH_MAX, HEIGHT);
        this.transform.position = new Vector3(Position.x, Position.y);
        IsAlive = true;
        isOutOfRange = false;
        SoundManager.instance.Mystery(true);
    }

    internal void Move()
    {
        if (!IsAlive)
            return;

        if (ticksToMove == 0)
        {
            Position.x += isGoingRight ? 1 : -1;
            Hitbox = new Rect(Position - (Size / 2), Size);
            this.transform.position = new Vector3(Position.x, Position.y);
            ticksToMove = MOVE_DELAY;

            if (Position.x < 0 || Position.x > GameplayController.WIDTH_MAX)
            {
                isOutOfRange = true;
                Die();
            }
        }
        ticksToMove--;
    }

    protected override void ExplosionSound()
    {
        if (isOutOfRange)
            SoundManager.instance.Mystery(false);
        else
            SoundManager.instance.MysteryExplosion();
    }
}
