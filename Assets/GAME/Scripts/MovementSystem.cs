using UnityEngine;

public class MovementSystem : MonoBehaviour
{
    private IMover _mover;

    public void SetMover(IMover newMover)
    {
        _mover = newMover;
    }

    public void HandleInput(MovementType inputType)
    {
        _mover?.HandleInput(inputType);
    }

    private void Update()
    {
        _mover?.Update();
    }

    [System.Serializable]
    public class MovementSystemContext
    {
        public Rigidbody Rb;
        public Transform MoverTansform;
        public Transform OrientationTransform;
        public Transform[] GroundCheckPoints;
        public float GroundCheckDistance;
        public LayerMask PlatformLayer;
    }
}
