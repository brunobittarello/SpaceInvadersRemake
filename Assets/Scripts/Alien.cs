using System.Collections.Generic;
using UnityEngine;

public class Alien : MonoBehaviour
{
    const int SIZE = 10;

    public Type Tipe;
    public List<Sprite> Sprites;
    public SpriteRenderer SpriteRenderer;

    internal Vector2Int Position;
    internal Vector2Int Size;
    internal Rect Hitbox;
    internal int SpriteIndex;
    internal bool IsAlive;

    public enum Type { A, B, C, S }

    private void Awake()
    {
        Size = new Vector2Int(SIZE, SIZE);
        SpriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
    }

    public void Spawn(Vector2Int position)
    {
        Position = position;
        Hitbox = new Rect(position - (Size / 2), Size);
        this.transform.position = new Vector3(position.x, position.y);
        IsAlive = true;
        this.gameObject.SetActive(true);
        SpriteIndex = 0;
    }

    public void Move(Vector2Int position)
    {
        Position = position;
        Hitbox = new Rect(position - (Size / 2), Size);
        //Hitbox = new Rect(position.x - HALF_SIZE, position.y - HALF_SIZE, HALF_SIZE * 2, HALF_SIZE * 2);
        this.transform.position = new Vector3(position.x, position.y);
        SpriteIndex = SpriteIndex == 1 ? 0 : 1;
        SpriteRenderer.sprite = Sprites[SpriteIndex];
    }

    internal void Die()
    {
        IsAlive = false;
        this.gameObject.SetActive(false);
        ExplosionSound();
    }

    protected virtual void ExplosionSound()
    {
        SoundManager.instance.AlienExplosion();
    }
}
