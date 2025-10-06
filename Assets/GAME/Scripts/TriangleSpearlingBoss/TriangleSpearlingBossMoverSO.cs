using UnityEngine;

namespace TriangleSpearlingBoss
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Movers/TriangleSpearlingBossMover")]
    public class TriangleSpearlingBossMoverSO : MoverMachineBaseSO
    {
        [SerializeField] private TriangleSpearlingBossMover _mover;
        protected override IMover Mover => _mover;
    }
}
