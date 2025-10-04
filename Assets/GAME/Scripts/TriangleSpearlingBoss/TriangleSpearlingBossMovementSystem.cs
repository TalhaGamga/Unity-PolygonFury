using R3;
using UnityEngine;

namespace TriangleSpearlingBoss
{
    public class TriangleSpearlingBossMovementSystem : MonoBehaviour
    {
        public Subject<Unit> TransitionStream = new();

        private IMover _mover;

        [SerializeField] private MoverMachineBaseSO _moverMachineSO;
    }
}
