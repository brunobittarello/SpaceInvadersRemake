using UnityEngine;

abstract class Projectile
{
    internal Vector2Int GridPosition;
    internal Vector2 VirtualPosition;
    internal bool IsAvailable{ get { return !IsActive && !IsExploding; } }
    internal bool IsActive;
    internal bool IsExploding;
    internal readonly Transform transform;
    protected Explosion explosion;

    public Projectile(Transform transform, GameObject explosionGo) {
        this.transform = transform;
        this.transform.gameObject.SetActive(false);
        explosion = new Explosion(explosionGo);
    }

    internal void Reset()
    {
        this.transform.gameObject.SetActive(false);
        explosion.Reset();
        IsActive = IsExploding = false;
    }

    internal void TickUpdate()
    {
        if (IsExploding)
            explosion.TickUpdate();
        else if (IsActive)
            Move();
    }

    abstract protected void Move();

    internal virtual void OnHit(Vector3 position, bool isAlien)
    {
        this.transform.gameObject.SetActive(false);
        IsActive = false;
        explosion.Spawn(AfterExplosion, position, isAlien);
        IsExploding = true;
    }

    internal virtual void AfterExplosion()
    {
        IsExploding = false;
    }

    internal void Clear()
    {
        this.transform.gameObject.SetActive(false);
        IsActive = false;
        IsExploding = false;
    }
}
