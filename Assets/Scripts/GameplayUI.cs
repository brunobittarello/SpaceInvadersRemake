using UnityEngine;

class GameplayUI
{
    const int CANNON_ICONS = 5;
    const int CANNON_ICONS_DIST = 16;
    const string SCORE_TEMPLATE = "{0}               {1}";
    const int MYSTERY_SCORE_TICKS = 45;
    const string PLAYER_MESSAGE = "PLAY    PLAYER <1 >";

    const int GAME_OVER_X = 84;
    const int GAME_OVER_Y = 200;
    const int GAME_OVER_DELAY = 12;

    GameObject[] cannonIcons;
    TextMesh textMenu;
    TextMesh textScore;
    TextMesh textLives;
    TextMesh textCharge;
    TextMesh textCredits;
    TextMesh textMystery;

    int mysteryTicks;
    int score;
    int topScore;
    bool isGameOver;
    bool isStarting;
    string startMessage;

    public GameplayUI(TextMesh menu, GameObject liveCannonGo, TextMesh score, TextMesh lives, TextMesh charge, TextMesh credits, TextMesh mysteryScore)
    {
        textMenu = menu;
        cannonIcons = new GameObject[CANNON_ICONS];
        cannonIcons[0] = liveCannonGo;
        textScore = score;
        textLives = lives;
        textCharge = charge;
        textCredits = credits;
        textMystery = mysteryScore;
    }

    internal void Reset()
    {
        textMystery.gameObject.SetActive(false);
        isStarting = isGameOver = false;
    }

    internal void TickUpdate()
    {
        if (isGameOver)
        {
            UpdateGameOver();
            return;
        }

        if (mysteryTicks > 0)
            mysteryTicks--;
        if (mysteryTicks == 0)
            textMystery.gameObject.SetActive(false);
    }

    void UpdateGameOver()
    {
        if (textMystery.text.Equals("GAME OVER"))
            return;

        mysteryTicks++;
        if (mysteryTicks % GAME_OVER_DELAY == 0)
            textMystery.text = "GAME OVER".Substring(0, mysteryTicks / GAME_OVER_DELAY);
    }

    internal void ShowMenu(bool active)
    {
        textMenu.gameObject.SetActive(active);
        textLives.gameObject.SetActive(!active);
        textCharge.gameObject.SetActive(!active);
        textMystery.gameObject.SetActive(false);
        if (!string.IsNullOrEmpty(startMessage))
            textMenu.text = startMessage;
    }

    internal void ShowMessage()
    {
        startMessage = textMenu.text;
        textMenu.text = PLAYER_MESSAGE;
    }

    internal void SetLives(int lives)
    {
        textLives.text = lives.ToString();
        for (int i = 0; i < CANNON_ICONS; i++)
        {
            var active = lives - 1 > i;
            if (cannonIcons[i] == null && !active)
                return;

            if (cannonIcons[i] == null && active)
            {
                cannonIcons[i] = GameObject.Instantiate(cannonIcons[i - 1], cannonIcons[i - 1].transform.parent);
                cannonIcons[i].transform.position += Vector3.right * CANNON_ICONS_DIST;
            }
            cannonIcons[i].SetActive(active);
        }
    }

    internal void SetCharges(int charge)
    {
        var text = "<";
        for (int i = 0; i < Cannon.SHOTS_TO_AIM; i++)
            text += (charge <= i) ?  "." : "|";
        text += ">";
        textCharge.text = text;
    }

    internal void SetScore(int score)
    {
        this.score = score;
        UpdateScore();
    }

    internal void SetTopScore(int topScore)
    {
        this.topScore = topScore;
        UpdateScore();
    }

    internal void SetMysteryScore(int score, Vector2Int position)
    {
        textMystery.gameObject.SetActive(true);
        textMystery.transform.position = new Vector3(position.x, position.y);
        textMystery.text = score.ToString();
        mysteryTicks = MYSTERY_SCORE_TICKS;
    }

    internal void SetGameOverMessage()
    {
        textMystery.gameObject.SetActive(true);
        textMystery.transform.position = new Vector3(GAME_OVER_X, GAME_OVER_Y);
        textMystery.text = "";
        isGameOver = true;
        mysteryTicks = 0;
    }

    void UpdateScore()
    {
        textScore.text = string.Format(SCORE_TEMPLATE, score.ToString("D4"), topScore.ToString("D4")).Replace("1", " 1");
    }
}
