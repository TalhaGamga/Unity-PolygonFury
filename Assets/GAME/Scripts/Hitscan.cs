using System;
using UnityEngine;

[Serializable]
public class Hitscan : IBullet
{
    [SerializeField] public LayerMask HitMask;

    public event Action<BulletHitInfo> OnBulletHit;

    public void Fire(Vector3 origin, Vector3 direction, float range)
    {
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, range, HitMask);

        if (hit.collider != null)
        {
            var hitInfo = new BulletHitInfo
            {
                Origin = origin,
                EndPoint = hit.point,
                Direction = direction,
                HitObject = hit.collider.gameObject
            };

            OnBulletHit?.Invoke(hitInfo);
        }
    }
}