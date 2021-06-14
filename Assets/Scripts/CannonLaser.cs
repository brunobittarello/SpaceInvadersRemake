using UnityEngine;

class CannonLaser : Laser
{
    const int SPEED = 3;
    bool isPierce;

    public CannonLaser(Transform transform, GameObject explosionGo) : base(transform, explosionGo) { speed = SPEED; }

    internal override bool Spawn(Vector2Int position)
    {
        return Spawn(position, false);
    }

    internal bool Spawn(Vector2Int position, bool isPierce)
    {
        if (!base.Spawn(position))
            return false;

        this.isPierce = isPierce;
        return true;
    }

    internal override void OnHit(Vector3 position, bool isAlien)
    {
        if (!isPierce || !isAlien)
        {
            base.OnHit(position, isAlien);
            return;
        }

        explosion.Spawn(AfterExplosion, position, isAlien);
    }
}
