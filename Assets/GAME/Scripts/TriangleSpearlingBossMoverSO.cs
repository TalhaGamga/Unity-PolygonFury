using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Movers/TriangleSpearlingBossMover")]
public class TriangleSpearlingBossMoverSO : MoverMachineBaseSO
{
    [SerializeField] private TriangleSpearingBossMover _mover;
    protected override IMover Mover => _mover;
}