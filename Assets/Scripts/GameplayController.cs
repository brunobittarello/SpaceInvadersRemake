using UnityEngine;
using System.Linq;
using System.Collections.Generic;

class GameplayController : MonoBehaviour
{
    const float FRAME_RATE = 1 / 60;
    internal const int HEIGHT_MAX = 220;
    internal const int WIDTH_MAX = 220;

    const int TICKS_TO_RESPAWN = 300;
    const int TICKS_TO_RESPAWN_HALF = TICKS_TO_RESPAWN / 2;

    const int START_LIVES = 3;

    const int SCORE_ALIEN_A = 10;
    const int SCORE_ALIEN_B = 20;
    const int SCORE_ALIEN_C = 30;
    const int SCORE_ALIEN_S = 100;
    const int SCORE_ALIEN_SS = 300;

    const int BONUS_FIRST_ALIEN_S = 23;
    const int BONUS_NEXT_ALIEN_S = 15;

    const int START_MESSAGE_TIME = 3;//seconds
    const int CANNON_SPAWN_TIME = 3;//seconds
    const int GAME_OVER_TIME = 3;//seconds

    public GameObject[] Barriers;

    [Header("Prefabs")]
    public Alien[] AlienPrefabs;
    public GameObject AlienLaserPrefab;

    [Header("Explosions")]
    public GameObject LaserExplosionGo;
    public GameObject MissileExplosionGo;

    [Header("Life")]
    public GameObject LifeCannonGo;

    [Header("UI")]
    public TextMesh TextMenu;
    public TextMesh TextScore;
    public TextMesh TextLives;
    public TextMesh TextCharge;
    public TextMesh TextCredits;
    public TextMesh TextMysteryScore;

    float timer;
    int tickCounter;
    int score;
    int topScore;
    int level;

    public Cannon Cannon;
    public FleetController Fleet;
    public IList<BarrierPart> BarrierParts;
    GameplayUI ui;
    ColisionManager colision;
    int lives;

    public GameState Status { get; private set; }

    Alien closest;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        Cannon.Init(LaserExplosionGo, MissileExplosionGo);
        Fleet = new FleetController(AlienPrefabs, AlienLaserPrefab, LaserExplosionGo);
        BarrierParts = new List<BarrierPart>();
        foreach (var barrier in Barriers)
            for (int i = 0; i < barrier.transform.childCount; i++)
                BarrierParts.Add(new BarrierPart(barrier.transform.GetChild(i).gameObject));
        ui = new GameplayUI(TextMenu, LifeCannonGo, TextScore, TextLives, TextCharge, TextCredits, TextMysteryScore);
        ui.SetScore(0);
        ui.SetTopScore(0);
        colision = new ColisionManager(Cannon, Fleet, BarrierParts, Score);
    }

    // Start is called before the first frame update
    void Start()
    {
        ShowMenu();
    }

    // Update is called once per frame
    void Update()
    {
        if (Status == GameState.Starting)
            UpdateStarting();
        if (Status == GameState.Playing)
            UpdatePlaying();
        if (Status == GameState.Destroyed)
            UpdateDestroyed();
        if (Status == GameState.GameOver)
            UpdateGameOver();
        if (Status == GameState.Menu && Input.anyKeyDown)
            NewGame();
    }

    void FixedUpdate()
    {
        if (Status != GameState.Playing)
            return;

        var x = 0;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            x = -1;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            x++;

        if (x != 0)
            Cannon.Move(x);
    }

    void ShowMenu()
    {
        Status = GameState.Menu;
        ui.ShowMenu(true);
        this.transform.GetChild(0).gameObject.SetActive(false);
    }

    void NewGame()
    {
        Status = GameState.Starting;
        timer = score = 0;
        level = 1;
        this.transform.GetChild(0).gameObject.SetActive(true);
        lives = START_LIVES;
        ui.Reset();
        ui.ShowMenu(false);
        ui.SetLives(lives);
        ui.SetScore(score);
    }

    void UpdateStarting()
    {
        LoadLevel();
        timer += Time.deltaTime;
        Status = GameState.Playing;

        if (timer > FRAME_RATE)
        {
            //Fleet.TickUpdate();
        }
    }

    void LoadLevel()
    {
        Cannon.Spawn(true);
        Fleet.Spawn(level);
        foreach (var barrierPart in BarrierParts)
            barrierPart.Reset();
    }

    void UpdatePlaying()
    {
        timer += Time.deltaTime;
        if (timer > FRAME_RATE)
        {
            Fleet.TickUpdate();
            if (Fleet.Status == FleetController.State.Arrived)
            {
                Fleet.TickUpdate();
                GameOver();
                return;
            } else if (Fleet.Status == FleetController.State.Over)
            {
                Cleared();
                return;
            }

            colision.VerifyColisions();
            if (!Cannon.IsAlive)
            {
                CannonDestroyed();
                return;
            }

            ui.TickUpdate();
            Cannon.TickUpdate();
            FindClosestAlien();
            timer -= FRAME_RATE;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Cannon.ShootLaser();
            ui.SetCharges(Cannon.shotCharge);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Cannon.ShootPiercingLaser();
            ui.SetCharges(Cannon.shotCharge);
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Cannon.ShootMissile(closest);
            ui.SetCharges(Cannon.shotCharge);
        }
    }

    void UpdateDestroyed()
    {
        tickCounter++;
        Cannon.TickUpdate();
        if (tickCounter == TICKS_TO_RESPAWN_HALF)
        {
            ui.SetLives(--lives);
            Cannon.gameObject.SetActive(false);
        }

        if (tickCounter == TICKS_TO_RESPAWN)
        {
            if (lives == 0)
            {
                GameOver();
                return;
            }
            Status = GameState.Playing;
            if (Fleet.MysteryAlien.IsAlive)
                SoundManager.instance.PauseResumeMystery(true);
            Cannon.Spawn();
        }
    }

    void Cleared()
    {
        level++;
        LoadLevel();
        SoundManager.instance.Mystery(false);
        Status = GameState.Starting;
    }

    void GameOver()
    {
        Status = GameState.GameOver;
        timer = 0;
        SoundManager.instance.Mystery(false);
        ui.SetGameOverMessage();
        if (score <= topScore)
            return;
        topScore = score;
        ui.SetTopScore(score);
    }

    void UpdateGameOver()
    {
        timer += Time.deltaTime;
        ui.TickUpdate();
        if (timer > GAME_OVER_TIME)
            ShowMenu();
    }

    void Score(Alien.Type type, Vector2Int position)
    {
        var amount = ScoreByAlienType(type); ;
        score += amount;
        ui.SetScore(score);
        if (type == Alien.Type.S)
            ui.SetMysteryScore(amount, position);
    }

    int ScoreByAlienType(Alien.Type type)
    {
        switch (type)
        {
            case Alien.Type.A: return SCORE_ALIEN_A;
            case Alien.Type.B: return SCORE_ALIEN_B;
            case Alien.Type.C: return SCORE_ALIEN_C;
            case Alien.Type.S:
                return (Cannon.shotCounter == BONUS_FIRST_ALIEN_S || Cannon.shotCounter - BONUS_FIRST_ALIEN_S % BONUS_NEXT_ALIEN_S == 0) ? SCORE_ALIEN_SS : SCORE_ALIEN_S;
        }
        return 0;
    }

    void FindClosestAlien()
    {
        if (!Cannon.Aim.activeSelf)
            return;

        if (Cannon.Missile.IsActive)
        {
            Cannon.AimAlien(closest.transform.position);
            return;
        }

        Vector2 pos = Cannon.Position;
        closest = Fleet.Aliens.Where(a => a.IsAlive).OrderBy(a => Vector2.SqrMagnitude(pos - a.Position)).FirstOrDefault();
        if (closest == null)
            return;
        Cannon.AimAlien(closest.transform.position);
    }

    void CannonDestroyed()
    {
        Status = GameState.Destroyed;
        tickCounter = 0;
        SoundManager.instance.PauseResumeMystery(false);
    }
}

enum GameState
{
    Menu,
    Starting,
    Playing,
    Destroyed,
    GameOver,
}
