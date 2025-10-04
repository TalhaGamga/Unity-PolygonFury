using R3;
using UnityEngine;

public class MovementSystem : MonoBehaviour
{
    public Subject<Unit> TransitionStream = new();

    private IMover _mover;

    [SerializeField] private MoverMachineBaseSO _moverMachineSO;
    [SerializeField] private ExternalSources _externalSources;

    private void Awake()
    {
        _mover = _moverMachineSO.GetMover();
        _mover.Construct(_externalSources);
        SetMoverMachine(_moverMachineSO.GetMover());
    }

    public void SetMoverMachine(IMover newMover)
    {
        _mover?.End();
        _mover = newMover;
        _mover.Init(TransitionStream);
    }

    public void HandleInput(InputSignal inputType)
    {
        _mover?.HandleInput(inputType);
    }

    private void Update()
    {
        _mover?.Update();
    }

    [System.Serializable]
    public class ExternalSources
    {
        public Rigidbody2D Rb;
        public Transform MoverTansform;
        public Transform OrientationTransform;
        public Transform[] GroundCheckPoints;
    }
}
