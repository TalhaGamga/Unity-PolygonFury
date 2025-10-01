using R3;
using UnityEngine;

public class MovementSystem : MonoBehaviour
{
    public Subject<Unit> TransitionStream = new();

    private IMachine _mover;
    [SerializeField] private ExternalSources _externalSources;

    [SerializeField] private RbMoverMachine _moverMachine;

    private void Awake()
    {
        SetMoverMachine(_moverMachine);
    }

    public void SetMoverMachine(IMachine newMover)
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
        public float GroundCheckDistance;
        public LayerMask PlatformLayer;
    }
}
