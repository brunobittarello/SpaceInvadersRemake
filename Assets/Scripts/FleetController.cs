using System.Collections.Generic;
using UnityEngine;

public class FleetController
{
    const int WIDTH = 11;//11
    const int HEIGHT = 5;//5

    const int DISTANCE = 16;
    const int SPEED = 3;
    const int STEP = 8;

    const int X_MIN = 0;
    const int X_MAX = 220;
    const int Y_ENDGAME = 33;

    const int LASER_MAX_COUNT = 3;
    const int SHOT_TICK_MIN = 30;
    const int SHOT_TICK_MAX = 90;

    const int TICKS_TO_MYSTERY_MIN = 1024;
    const int TICKS_TO_MYSTERY_MAX = 4096;

    static Vector2Int START_POS = new Vector2Int(32, 192);

    Alien[] AlienPrefabs;
    GameObject AlienLaserPrefab;
    GameObject ExplosionPrefab;

    int current;
    internal IList<Alien> Aliens;
    internal IList<Laser> AlienLasers;
    internal MysteryAlien MysteryAlien;
    int direction = 1;
    int step;
    int nextShot;
    int nextMystery;
    int aliensAlive;
    int minX;
    int maxX;
    int minY;
    int soundTick;
    internal State Status { get; private set; }

    internal enum State { Moving, Arrived, Over }

    public FleetController(Alien[] alienPrefabs, GameObject alienLaserPrefab, GameObject explosionPrefab)
    {
        AlienPrefabs = alienPrefabs;
        AlienLaserPrefab = alienLaserPrefab;
        ExplosionPrefab = explosionPrefab;
        Init();
    }

    void Init()
    {
        var pool = new GameObject("AlienPool").transform;
        pool.transform.SetParent(ExplosionPrefab.transform.parent.parent);
        Aliens = new List<Alien>();
        for (int y = HEIGHT - 1; y > -1; y--)
            for (int x = 0; x < WIDTH; x++)
            {
                var idPrefab = (y == 0) ? 2 : (y < 3) ? 1 : 0;
                var newAlien = GameObject.Instantiate(AlienPrefabs[idPrefab], pool);
                Aliens.Add(newAlien);
                //newAlien.Spawn(new Vector2Int(START_POS.x + (DISTANCE * x), START_POS.y - (DISTANCE * y)));
            }

        MysteryAlien = (MysteryAlien)GameObject.Instantiate(AlienPrefabs[3], pool);

        pool = GameObject.Find("Projectiles").transform;
        AlienLasers = new List<Laser>();
        for (int x = 0; x < LASER_MAX_COUNT; x++)
        {
            var newAlienLaser = GameObject.Instantiate(AlienLaserPrefab, pool);
            var newExploson = GameObject.Instantiate(ExplosionPrefab, ExplosionPrefab.transform.parent);
            AlienLasers.Add(new Laser(newAlienLaser.transform, newExploson));
        }
    }

    internal void Spawn(int level)
    {
        soundTick = current = 0;
        aliensAlive = Aliens.Count;
        Status = State.Moving;
        var startPosition = StartPosition(level);
        ResetMysteryTimer();
        for (int y = HEIGHT - 1; y > -1; y--) { 
            var posY = startPosition.y - (DISTANCE * (HEIGHT - (y + 1)));
            for (int x = 0; x < WIDTH; x++)
                AlienByGrid(x, y).Spawn(new Vector2Int(startPosition.x + (DISTANCE * x), posY));
        }

        for (int i = 0; i < AlienLasers.Count; i++)
            AlienLasers[i].Reset();

        MysteryAlien.gameObject.SetActive(false);
    }

    internal void TickUpdate()
    {
        VerifyFleetStatus();
        if (Status == State.Over)
            return;

        Move();
        TryShoot();
        TryToMisteryAlien();
        for (int x = 0; x < AlienLasers.Count; x++)
            AlienLasers[x].TickUpdate();
    }

    void VerifyFleetStatus()
    {
        var IsOneAlive = false;
        aliensAlive = 0;
        minX = int.MaxValue;
        maxX = int.MinValue;
        minY = int.MaxValue;
        foreach (var alien in Aliens)
        {
            if (!alien.IsAlive)
                continue;

            aliensAlive++;
            if (!IsOneAlive)
                IsOneAlive = true;

            if (alien.Position.x < minX)
                minX = alien.Position.x;

            if (alien.Position.x > maxX)
                maxX = alien.Position.x;

            if (alien.Position.y < minY)
                minY = alien.Position.y;
        }

        if (!IsOneAlive)
            Status = State.Over;
    }

    void Move()
    {
        MysteryAlien.Move();
        for (; current < Aliens.Count; current++)
            if (Aliens[current].IsAlive)
            {
                Aliens[current].Move(Aliens[current].Position + new Vector2Int(SPEED * direction, step));
                current++;
                return;
            }

        soundTick++;
        var soundRate = (aliensAlive > 10) ? 2 : (aliensAlive > 5) ? 3 : 4;
        if (soundTick >= soundRate)
        {
            SoundManager.instance.NextFleetSound();
            soundTick = 0;
        }
        VerifyFleetPosition();
    }

    void VerifyFleetPosition()
    {
        current = 0;
        if (minX <= X_MIN || maxX >= X_MAX)
        {
            if (minY - STEP <= Y_ENDGAME)
                Status = State.Arrived;

            direction *= -1;
            step = -STEP;
        }
        else
            step = 0;
    }

    void TryShoot()
    {
        nextShot--;
        if (nextShot > 0)
            return;

        nextShot = Random.Range(SHOT_TICK_MIN, SHOT_TICK_MAX);
        var laser = GetAvailableLaser();

        if (laser == null)
            return;

        var column = Random.Range(0, WIDTH);
        for (int y = 0; y < HEIGHT; y++) { 
            var alien = AlienByGrid(column, y);
            if (alien.IsAlive)
            {
                Shoot(laser, alien.Position);
                return;
            }
        }
    }

    void TryToMisteryAlien()
    {
        nextMystery--;
        if (nextMystery == 0)
        {
            MysteryAlien.Spawn();
            ResetMysteryTimer();
        }
    }

    Laser GetAvailableLaser()
    {
        for (int i = 0; i < LASER_MAX_COUNT; i++)
            if (AlienLasers[i].IsAvailable)
                return AlienLasers[i];
        return null;
    }

    void Shoot(Laser laser, Vector2Int position)
    {
        laser.Spawn(position);
    }

    void ResetMysteryTimer()
    {
        nextMystery = Random.Range(TICKS_TO_MYSTERY_MIN, TICKS_TO_MYSTERY_MAX);
    }

    Vector2Int StartPosition(int level)
    {
        return START_POS + Vector2Int.down * STEP * Mathf.Clamp(level, 1 , 6);
    }

    Alien AlienByGrid(int x, int y)
    {
        return Aliens[x + (y * WIDTH)];
    }
}
