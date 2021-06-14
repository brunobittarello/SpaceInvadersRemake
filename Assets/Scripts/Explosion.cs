using System;
using UnityEngine;

class Explosion
{
    const int EXPLOSION_TIME = 16;

    GameObject gameobject;
    Action callback;
    int tickCounter;

    public Explosion(GameObject gameobject)
    {
        this.gameobject = gameobject;
        this.gameobject.SetActive(false);
    }

    internal void Reset()
    {
        this.gameobject.SetActive(false);
    }

    internal void Spawn(Action callback, Vector3 position, bool IsAlien)
    {
        gameobject.SetActive(true);
        this.callback = callback;
        this.gameobject.transform.position = position;
        this.gameobject.transform.localScale = Vector3.one * (IsAlien ? 1 : 0.75f);
        tickCounter = 0;
    }

    internal void TickUpdate()
    {
        if (!gameobject.activeSelf)
            return;

        tickCounter++;
        if (tickCounter == EXPLOSION_TIME)
        {
            callback();
            gameobject.SetActive(false);
        }
    }
}
