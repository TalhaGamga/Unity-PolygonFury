using UnityEngine;

namespace TriangleSpearlingBoss
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Movers/TriangleSpearlingBossMover")]
    public class TriangleSpearlingBossMoverSO : MoverMachineBaseSO
    {
        [SerializeField] private BossMover _mover;
        protected override IMover Mover => _mover;
    }
}
