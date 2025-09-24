using DevVorpian;
using UnityEngine;

public class RbMover : IMover
{
    private MovementSystem.MovementSystemContext _context;
    private StateMachine<MovementType> _stateMachine;

    public RbMover(MovementSystem.MovementSystemContext context)
    {
        _context = context;
    }

    public void Init()
    {

    }

    public void HandleInput(MovementType movementType)
    {
        _stateMachine.SetState(movementType);
    }

    public void End()
    {
    }

    public void Update()
    {
    }

    private bool IsGrounded()
    {
        foreach (var checkPoint in _context.GroundCheckPoints)
        {
            return Physics2D.OverlapCircle(checkPoint.position, _context.GroundCheckDistance, _context.PlatformLayer);
        }

        return false;
    }
}