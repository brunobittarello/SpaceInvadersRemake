using System;
using System.Collections.Generic;
using UnityEngine;

class ColisionManager
{
    readonly Cannon Cannon;
    readonly FleetController Fleet;
    readonly IList<BarrierPart> BarrierParts;
    readonly Action<Alien.Type, Vector2Int> Score;

    readonly Projectile[] allProjectiles;

    public ColisionManager(Cannon cannon, FleetController fleet, IList<BarrierPart> barrierParts, Action<Alien.Type, Vector2Int> score)
    {
        Cannon = cannon;
        Fleet = fleet;
        BarrierParts = barrierParts;
        Score = score;

        var projectiles = new List<Projectile>();
        projectiles.Add(cannon.Laser);
        projectiles.Add(cannon.Missile);
        projectiles.AddRange(fleet.AlienLasers);
        allProjectiles = projectiles.ToArray();
    }

    internal void VerifyColisions()
    {
        VerifyProjectiles();
        VerifyAlienBarrierCollisions();
    }

    void VerifyProjectiles()
    {
        var isAgaistAlien = false;
        var hitPos = Vector2.zero;
        foreach (var projectile in allProjectiles)
        {
            if (VerifyProjectile(projectile, out hitPos, out isAgaistAlien))
            {
                projectile.OnHit(hitPos, isAgaistAlien);
            }
        }
    }

    bool VerifyProjectile(Projectile projectile, out Vector2 hitPos, out bool isAgaistAlien)
    {
        hitPos = Vector2.zero;
        isAgaistAlien = false;
        if (projectile.IsActive == false)
            return false;

        var pos = new Vector2Int((int)projectile.transform.position.x, (int)projectile.transform.position.y);
        hitPos = pos;
        if (IsOffScreenLimits(pos))
        {
            return true;
        }

        if (VerifyBarrierCollision(pos))
            return true;

        if (projectile is CannonLaser || projectile is Missile)
        {
            if (VerifyCollisionAgainstAliens(pos, out hitPos))
            {
                isAgaistAlien = true;
                return true;
            }
        }
        else if (VerifyCollisionAgainstCannon(pos, out hitPos))
            return true;
        return false;
    }

    bool VerifyCollisionAgainstCannon(Vector2 pos, out Vector2 hitpos)
    {
        hitpos = pos;
        if (Cannon.Hitbox.Contains(pos))
        {
            Cannon.Explode();
            return true;
        }
        return false;
    }

    bool IsOffScreenLimits(Vector2 position)
    {
        return position.y < 0 || position.y >= GameplayController.HEIGHT_MAX || position.x < 0 || position.x > GameplayController.WIDTH_MAX;
    }

    bool VerifyCollisionAgainstAliens(Vector2 pos, out Vector2 hitpos)
    {
        foreach (var alien in Fleet.Aliens)
        {
            if (alien.IsAlive && alien.Hitbox.Contains(pos))
            {
                alien.Die();
                Score(alien.Tipe, Vector2Int.zero);
                hitpos = alien.Position;
                return true;
            }
        }

        if (Fleet.MysteryAlien.IsAlive && Fleet.MysteryAlien.Hitbox.Contains(pos))
        {
            Fleet.MysteryAlien.Die();
            Score(Fleet.MysteryAlien.Tipe, new Vector2Int((int)pos.x, (int)pos.y));
            hitpos = Fleet.MysteryAlien.Position;
            return true;
        }
        hitpos = pos;
        return false;
    }

    void VerifyAlienBarrierCollisions()
    {
        foreach (var alien in Fleet.Aliens)
        {
            if (!alien.IsAlive)
                continue;

            foreach (var barrierPart in BarrierParts)
            {
                if (barrierPart.IsActive && alien.Hitbox.Overlaps(barrierPart.Hitbox))
                    barrierPart.Remove();
            }
        }
    }

    bool VerifyBarrierCollision(Vector2 point)
    {
        foreach (var barrierPart in BarrierParts)
        {
            if (barrierPart.IsActive && barrierPart.Hitbox.Contains(point))
            {
                barrierPart.TakeDamage();
                return true;
            }
        }
        return false;
    }
}
