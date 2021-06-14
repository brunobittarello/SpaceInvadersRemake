using System.Collections.Generic;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    const int SPRITE_CANNON = 0;
    const int SPRITE_EXPLOSION = 1;

    const int SPAWN_POS_X = 0;
    const int SPAWN_POS_Y = 32;

    const int SPEED = 2;
    const int X_MIN = 0;
    const int X_MAX = 220;

    internal const int SHOTS_TO_AIM = 4;

    const int DEAD_FRAME = 30;

    public GameObject LaserGO;
    public GameObject MissileGO;
    internal CannonLaser Laser;
    internal Missile Missile;
    public GameObject Aim;
    public List<Sprite> Sprites;
    public SpriteRenderer SpriteRenderer;
    internal Rect Hitbox;

    internal Vector2Int Position;
    internal Vector2Int Size;
    internal bool IsAlive;
    internal byte shotCharge { get; private set; }
    internal int shotCounter { get; private set; }
    byte tickCounter;

    void Awake()
    {
        Size = new Vector2Int(12, 8);
        SpriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
    }

    internal void Init(GameObject laserExplosionGo, GameObject missileExplosionGo)
    {
        Laser = new CannonLaser(LaserGO.transform, laserExplosionGo);
        Missile = new Missile(MissileGO.transform, missileExplosionGo);
        Missile.OnDoneExploding = CheckChargeAvailable;
    }

    internal void TickUpdate()
    {
        if (IsAlive)
        {
            Laser.TickUpdate();
            Missile.TickUpdate();
        }
        else
            DeathAnimation();
    }

    internal void Spawn(bool isNewGame = false)
    {
        if (isNewGame)
        {
            Aim.SetActive(false);
            shotCounter = 0;
            shotCharge = 0;
        }

        Position = new Vector2Int(SPAWN_POS_X, SPAWN_POS_Y);
        this.gameObject.SetActive(true);
        this.transform.position = new Vector3(Position.x, Position.y);
        SpriteRenderer.sprite = Sprites[SPRITE_CANNON];
        IsAlive = true;
        Laser.Reset();
        Missile.Reset();
    }

    internal void Move(int delta)
    {
        if (!IsAlive)
            return;
        Position.x = Mathf.Clamp(Position.x + delta * SPEED, X_MIN, X_MAX);
        Hitbox = new Rect(Position - (Size / 2), Size);
        this.transform.position = new Vector3(Position.x, Position.y);
    }

    internal void ShootLaser()
    {
        if (!IsAlive)
            return;

        if (Laser.Spawn(Position))
        {
            SoundManager.instance.CannonShot();
            shotCounter++;
            shotCharge++;
        }

        if (shotCharge >= SHOTS_TO_AIM)
        {
            shotCharge = SHOTS_TO_AIM;
            Aim.SetActive(true);
        }
    }

    internal void ShootPiercingLaser()
    {
        if (!IsAlive || !IsCharged())
            return;

        if (Laser.Spawn(Position, true))
        {
            SoundManager.instance.CannonShot();
            shotCounter++;
            shotCharge = 0;
            Aim.SetActive(false);
        }
    }

    internal void ShootMissile(Alien target)
    {
        if (!IsAlive || Aim.activeInHierarchy == false || target == null)
            return;

        if (Missile.Launch(this.transform.position, target))
        {
            SoundManager.instance.CannonShot();
            shotCharge = 0;
        }
    }

    internal void AimAlien(Vector3 position)
    {
        Aim.transform.position = new Vector3(position.x, position.y);
    }

    internal void Explode()
    {
        SpriteRenderer.sprite = Sprites[SPRITE_EXPLOSION];
        IsAlive = false;
        Laser.Clear();
        Missile.Clear();
        SoundManager.instance.CannonExplosion();
    }

    internal bool IsCharged()
    {
        return shotCharge == SHOTS_TO_AIM;
    }

    void CheckChargeAvailable()
    {
        if (!IsCharged())
            Aim.SetActive(false);
    }

    void DeathAnimation()
    {
        tickCounter++;
        if (tickCounter == DEAD_FRAME)
        {
            SpriteRenderer.flipX = !SpriteRenderer.flipX;
            tickCounter = 0;
        }
    }
}
