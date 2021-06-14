using System;
using UnityEngine;

internal class Missile : Projectile
{
    const int SPEED = 2;

    Alien Target;
    Vector2 velocity;
    Vector2 direction;
    internal Action OnDoneExploding;

    public Missile(Transform transform, GameObject explosionGo) : base(transform, explosionGo) { }

    internal bool Launch(Vector3 startPos, Alien target)
    {
        if (!IsAvailable)
            return false;

        IsActive = true;
        transform.position = startPos;
        transform.gameObject.SetActive(true);
        Target = target;
        velocity = Vector2.zero;
        return true;
    }

    protected override void Move()
    {
        if (Target.IsAlive)
            direction = (Target.Position - (Vector2)transform.position).normalized;

        velocity = (velocity + direction * 0.2f).normalized * SPEED;
        this.transform.position += (Vector3)velocity;
    }

    internal override void AfterExplosion()
    {
        base.AfterExplosion();
        if (OnDoneExploding != null)
            OnDoneExploding();
    }
}
