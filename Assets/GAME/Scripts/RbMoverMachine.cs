using DevVorpian;
using UnityEngine;

[System.Serializable]
public class RbMoverMachine : IMachine
{
    private StateMachine<PlayerAction> _stateMachine;
    [SerializeField] private Context _context;
    
    private InputSignal _cachedInputSignal;
    
    public RbMoverMachine(MovementSystem.ExternalSources external)
    {
        _context = new Context();

        _context.MoverTransform = external.MoverTansform;
        _context.OrientationTransform = external.OrientationTransform;
        _context.Rb = external.Rb;
        _context.GroundCheckDistance = external.GroundCheckDistance;
        _context.PlatformLayer = external.PlatformLayer;
    }

    public void Init()
    {
        var idle = new ConcreteState("Idle");
        var move = new ConcreteState("Move");
        var jump = new ConcreteState("Jump");
        var fall = new ConcreteState("Fall");
        var dash = new ConcreteState("Dash");
        var neutral = new ConcreteState("Neutral"); // to be used

        var toIdle = new StateTransition<PlayerAction>(null, idle, PlayerAction.Idle, () => _context.MoveInput.magnitude == 0 && _context.IsGrounded && _context.CurrentAction != PlayerAction.Jump);
        var toMove = new StateTransition<PlayerAction>(null, move, PlayerAction.Move, () => _context.MoveInput.magnitude > 0 && _context.IsGrounded && _context.CurrentAction != PlayerAction.Jump);
        var toJump = new StateTransition<PlayerAction>(null, jump, PlayerAction.Jump, () => _context.IsGrounded, onTransition: () => Debug.Log("Transitioning to jump"));
        var toDash = new StateTransition<PlayerAction>(null, dash, PlayerAction.Dash, () => true);
        var toFall = new StateTransition<PlayerAction>(null, fall, PlayerAction.Fall, () => !_context.IsGrounded && (_context.CurrentAction != PlayerAction.Dash) && _context.VerticalVelocity < 0);
        var fallToNeutral = new StateTransition<PlayerAction>(fall, neutral, PlayerAction.Neutral, () => _context.IsGrounded);
        _stateMachine = new StateMachine<PlayerAction>();
        _stateMachine.OnTransitionedAutonomously.AddListener(()=>handleInput());

        _stateMachine.AddIntentBasedTransition(toMove);
        _stateMachine.AddIntentBasedTransition(toJump);
        _stateMachine.AddIntentBasedTransition(toDash);

        _stateMachine.AddAutonomicTransition(toIdle);
        _stateMachine.AddAutonomicTransition(toFall);
        _stateMachine.AddAutonomicTransition(fallToNeutral);

        #region OnEnter
        move.OnEnter.AddListener(() =>
        {
            setContextState(PlayerAction.Move);
            setHorizontalSpeed(_context.HorizontalMovableSpeed);
            constraintRbAxisY(true);
        });

        idle.OnEnter.AddListener(() =>
        {
            setContextState(PlayerAction.Idle);
        });

        jump.OnEnter.AddListener(() =>
        {
            setContextState(PlayerAction.Jump);
            constraintRbAxisY(false);
            setVerticalVelocity(calculateJumpVelocity());
            setHorizontalSpeed(_context.AirborneMovementSpeed);
        });

        fall.OnEnter.AddListener(() =>
        {
            setContextState(PlayerAction.Fall);
            constraintRbAxisY(false);
            setHorizontalSpeed(_context.AirborneMovementSpeed);
        });

        neutral.OnEnter.AddListener(() =>
        {
            setContextState(PlayerAction.Neutral);
        });

        dash.OnEnter.AddListener(() =>
        {
            setContextState(PlayerAction.Dash);
            constraintRbAxisY(true);
            dashing();
        });
        #endregion

        #region Update
        idle.OnUpdate.AddListener(() =>
        {
        });

        move.OnUpdate.AddListener(() =>
        {
            setCharacterOrientator();
            blendHorizontalVelocity();
            applyRbMovement();
        });

        jump.OnUpdate.AddListener(() =>
        {
            setCharacterOrientator();
            blendHorizontalVelocity();
            handleAirborneMovement();
            handleGravity();
        });

        fall.OnUpdate.AddListener(() =>
        {
            setCharacterOrientator();
            blendHorizontalVelocity();
            handleGravity();
            handleAirborneMovement();
        });
        #endregion

        #region OnExit
        #endregion

        _stateMachine.SetState(PlayerAction.Idle);

        setContextGravity();
    }

    public void HandleInput(InputSignal inputSignal)
    {
        var dir = inputSignal.Value;
        _context.MoveInput = dir != null ? (Vector2)dir : Vector2.zero;
        _stateMachine.SetState(inputSignal.Action);
        _cachedInputSignal= inputSignal;    
    }

    private void handleInput()
    {
        HandleInput(_cachedInputSignal);
    }

    private void setContextState(PlayerAction actionType)
    {
        _context.CurrentAction = actionType;
    }

    private void blendHorizontalVelocity()
    {
        var desiredSpeed = _context.HorizontalVelocity * _context.MoveInput.x;
        _context.HorizontalCurrentVelocity = Mathf.MoveTowards(_context.HorizontalCurrentVelocity, desiredSpeed, _context.Acceleration);
    }

    private void applyRbMovement()
    {
        _context.Rb.linearVelocity = new Vector2(_context.HorizontalCurrentVelocity, _context.VerticalVelocity);
    }

    private void constraintRbAxisY(bool isAllowed)
    {
        _context.Rb.constraints = isAllowed
            ? RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionY
            : RigidbodyConstraints2D.FreezeRotation;
    }

    private void dashing()
    {
        _context.Rb.linearVelocity = new Vector3(0, 0, _context.DashSpeed * _context.LastFaceX);
        _context.VerticalVelocity = 0;
    }


    private void setHorizontalSpeed(float velocity)
    {
        _context.HorizontalVelocity = velocity;
    }

    private void setVerticalVelocity(float velocity)
    {
        _context.VerticalVelocity = velocity;
    }
    private void handleAirborneMovement()
    {
        _context.Rb.linearVelocity = new Vector2(_context.HorizontalCurrentVelocity, _context.VerticalVelocity);
    }

    private void handleGravity()
    {
        _context.VerticalVelocity -= _context.Gravity * Time.deltaTime;
    }

    private float calculateJumpVelocity()
    {
        return _context.Gravity * _context.JumpTimeToPeak;
    }

    private void setCharacterOrientator()
    {
        float x = _context.MoveInput.x;

        if (Mathf.Abs(x) > _context.FaceDeadzone)
        {
            float sign = Mathf.Sign(x);
            if (sign != 0f) _context.LastFaceX = (int)sign;
        }

        float target = _context.LastFaceX;

        Quaternion targetLocalRot = (target > 0f)
            ? Quaternion.Euler(0f, 0f, 0f)
            : Quaternion.Euler(0f, 180f, 0f);

        _context.OrientationTransform.localRotation = targetLocalRot;
    }

    private void setContextGravity()
    {
        _context.Gravity = (2f * _context.JumpHeight) / (Mathf.Pow(_context.JumpTimeToPeak, 2));
    }

    public void End()
    {
    }

    public void Update()
    {
        _stateMachine?.Update();
        _context.IsGrounded = isGrounded();
        Debug.Log(_context.MoveInput.magnitude);
    }

    private bool isGrounded()
    {
        foreach (var checkPoint in _context.GroundCheckPoints)
        {
            DrawDebugCircle(checkPoint.position, _context.GroundCheckDistance, Color.green);

            if (Physics2D.OverlapCircle(checkPoint.position, _context.GroundCheckDistance, _context.PlatformLayer))
                return true;
        }

        return false;
    }

    private void DrawDebugCircle(Vector3 center, float radius, Color color, int segments = 20)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(Mathf.Cos(0f), Mathf.Sin(0f)) * radius;

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 nextPoint = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            Debug.DrawLine(prevPoint, nextPoint, color);
            prevPoint = nextPoint;
        }
    }

    [System.Serializable]
    private class Context
    {
        public PlayerAction CurrentAction;
        public float Gravity;
        public Rigidbody2D Rb;
        public Vector2 MoveInput;
        public Transform MoverTransform;
        public Transform OrientationTransform;
        public Transform[] GroundCheckPoints;
        public LayerMask PlatformLayer;
        public float GroundCheckDistance;

        public float HorizontalMovableSpeed;
        public float HorizontalCurrentVelocity;
        public float Acceleration;

        public float DashSpeed;

        public int LastFaceX;
        public float VerticalVelocity;
        public float HorizontalVelocity;
        public float JumpTimeToPeak;
        public float FaceDeadzone;
        public float JumpHeight;
        public float AirborneMovementSpeed;
        public bool IsGrounded;

        public float JumpSpeed;
    }
}