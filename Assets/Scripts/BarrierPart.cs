using UnityEngine;

class BarrierPart
{
    const int MAX_DAMAGE = 3;
    const int SIZE = 8;

    readonly public Rect Hitbox;
    readonly SpriteRenderer spriteRenderer;
    GameObject gameObject;
    int damage;
    internal bool IsActive { get { return gameObject.activeSelf; } }

    public BarrierPart(GameObject gameObject)
    {
        this.gameObject = gameObject;
        Hitbox = new Rect(gameObject.transform.position, Vector2.one * SIZE);
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    internal void TakeDamage()
    {
        damage++;
        if (damage == MAX_DAMAGE) { 
            this.gameObject.SetActive(false);
            return;
        }
        UpdateVisual();
    }

    internal void Remove()
    {
        this.gameObject.SetActive(false);
    }

    internal void Reset()
    {
        this.gameObject.SetActive(true);
        damage = 0;
        UpdateVisual();
    }

    void UpdateVisual()
    {
        spriteRenderer.color = ColorByDamage();
    }

    Color ColorByDamage()
    {
        switch (damage)
        {
            case 0: return Color.green;
            case 1: return Color.yellow;
            case 2: return Color.red;
        }
        return Color.white;
    }
}
