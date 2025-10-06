using R3;
using UnityEngine;

public class TriangleSpearlingBossInputHandler : MonoBehaviour, IInputHandler
{
    public BehaviorSubject<InputSnapshot> InputSnapshotStream => throw new System.NotImplementedException();


    //private Vector3 scanTarget()
    //{
    //    Collider2D target = Physics2D.OverlapCircle(
    //        _context.SpearTransform.position,
    //        _context.TargetScanRadius,
    //        _context.TargetLayer
    //    );

    //    Debug.DrawLine(_context.SpearTransform.position,
    //                   _context.SpearTransform.position + Vector3.right * _context.TargetScanRadius,
    //                   Color.red, 0.1f);

    //    drawWireSphere(_context.SpearTransform.position, _context.TargetScanRadius, Color.green);

    //    return target != null ? target.transform.position : Vector3.zero;
    //}

    //private void drawWireSphere(Vector3 center, float radius, Color color, int segments = 20)
    //{
    //    float angleStep = 360f / segments;
    //    Vector3 prevPoint = center + new Vector3(Mathf.Cos(0f), Mathf.Sin(0f)) * radius;

    //    for (int i = 1; i <= segments; i++)
    //    {
    //        float angle = i * angleStep * Mathf.Deg2Rad;
    //        Vector3 nextPoint = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
    //        Debug.DrawLine(prevPoint, nextPoint, color);
    //        prevPoint = nextPoint;
    //    }
    //}
}