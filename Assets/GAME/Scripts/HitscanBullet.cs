using System;
using UnityEngine;

[RequireComponent(typeof(IBulletVisualizer))]
public class HitscanBullet : MonoBehaviour, IBullet
{
    public event Action<BulletHitInfo> OnBulletHit;
    [SerializeField] private LayerMask _hitMask;

    public void Fire(Vector3 origin, Vector3 direction, float range)
    {
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, range, _hitMask);

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