using UnityEngine;

namespace TriangleSpearlingBoss
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Movers/TriangleSpearlingInputDrivenBossMover")]
    public class TriangleSpearlingBossInputDrivenMoverSO : MoverMachineBaseSO
    {
        [SerializeField] private TriangleSpearlingBossInputDrivenMover _mover;
        protected override IMover Mover => _mover;
    }
}
