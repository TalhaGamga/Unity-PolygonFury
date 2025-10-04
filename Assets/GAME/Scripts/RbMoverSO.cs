using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Movers/RbMover")]
public class RbMoverSO : MoverMachineBaseSO
{
    [SerializeField] private RbMover _mover;

    protected override IMover Mover => _mover;
}