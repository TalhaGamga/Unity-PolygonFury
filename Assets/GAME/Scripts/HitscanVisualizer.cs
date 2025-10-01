using System.Collections;
using UnityEngine;

public class HitscanVisualizer : MonoBehaviour, IBulletVisualizer
{
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private HitscanBullet _hitscanBullet;
    public IBullet Bullet => _hitscanBullet;

    private void OnEnable()
    {
        Bullet.OnBulletHit += visualizeHit;
    }

    private void OnDisable()
    {
        Bullet.OnBulletHit -= visualizeHit;
    }

    private void visualizeHit(BulletHitInfo hitInfo)
    {
        StartCoroutine(VisualizeHitCor(hitInfo));
    }

    private IEnumerator VisualizeHitCor(BulletHitInfo hitInfo)
    {
        _lineRenderer.SetPosition(0, hitInfo.Origin);
        _lineRenderer.SetPosition(1, hitInfo.EndPoint);
        _lineRenderer.enabled = true;
        yield return new WaitForSeconds(0.015f);
        _lineRenderer.enabled = false;
    }
}