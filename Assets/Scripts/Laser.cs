using UnityEngine;

class Laser : Projectile
{
    const int SPEED = -2;
    protected int speed;

    public Laser(Transform transform, GameObject explosionGo) : base(transform, explosionGo) { speed = SPEED; }

    internal virtual bool Spawn(Vector2Int position)
    {
        if (!IsAvailable)
            return false;

        IsActive = true;
        VirtualPosition = GridPosition = position;
        this.transform.position = new Vector3(GridPosition.x, GridPosition.y);
        transform.gameObject.SetActive(true);
        return true;
    }

    protected override void Move()
    {
        VirtualPosition.y += speed;
        this.transform.position = VirtualPosition;
    }
}
