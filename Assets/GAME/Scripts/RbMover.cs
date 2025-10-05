using DevVorpian;
using R3;
using UnityEngine;

[System.Serializable]
public class RbMover : IMover
{
    private StateMachine<CharacterAction> _stateMachine;
    [SerializeField] private Context _context;

    private InputSignal _cachedInputSignal;

    public void Construct(MovementSystem.Context externalSources)
    {
        _context.Rb = externalSources.Rb;
        _context.MoverTransform = externalSources.MoverTansform;
        _context.OrientationTransform = externalSources.OrientationTransform;
        _context.GroundCheckPoints = externalSources.GroundCheckPoints;
    }

    public void Init(Subject<Unit> transitionStream)
    {
        var idle = new ConcreteState("Idle");
        var move = new ConcreteState("Move");
        var jump = new ConcreteState("Jump");
        var fall = new ConcreteState("Fall");
        var dash = new ConcreteState("Dash");
        var neutral = new ConcreteState("Neutral"); // to be used

        var toIdle = new StateTransition<CharacterAction>(null, idle, CharacterAction.Idle, () => _context.MoveInput.magnitude == 0 && _context.IsGrounded && _context.CurrentAction != CharacterAction.Jump && _context.CurrentAction != CharacterAction.Dash);
        var toMove = new StateTransition<CharacterAction>(null, move, CharacterAction.Move, () => _context.MoveInput.magnitude > 0 && _context.IsGrounded && _context.CurrentAction != CharacterAction.Jump && _context.CurrentAction != CharacterAction.Dash);
        var toJump = new StateTransition<CharacterAction>(null, jump, CharacterAction.Jump, () => _context.IsGrounded);
        var toDash = new StateTransition<CharacterAction>(null, dash, CharacterAction.Dash, () => true);
        var toFall = new StateTransition<CharacterAction>(null, fall, CharacterAction.Fall, () => !_context.IsGrounded && (_context.CurrentAction != CharacterAction.Dash) && _context.VerticalVelocity <= 0);
        var fallToNeutral = new StateTransition<CharacterAction>(fall, neutral, CharacterAction.Neutral, () => _context.IsGrounded);
        var dashToNeutral = new StateTransition<CharacterAction>(dash, neutral, CharacterAction.Neutral, () => _context.HasDashEnded);

        var jumpToFallWithJumpCancel = new StateTransition<CharacterAction>(jump, fall, CharacterAction.JumpCancel, onTransition: () =>
        {
            debuffJumpingRise();
        });

        _stateMachine = new StateMachine<CharacterAction>();
        _stateMachine.OnTransitionedAutonomously.AddListener(() => transitionStream.OnNext(Unit.Default));

        _stateMachine.AddIntentBasedTransition(toMove);
        _stateMachine.AddIntentBasedTransition(toJump);
        _stateMachine.AddIntentBasedTransition(toDash);
        _stateMachine.AddIntentBasedTransition(jumpToFallWithJumpCancel);

        _stateMachine.AddAutonomicTransition(toIdle);
        _stateMachine.AddAutonomicTransition(toFall);
        _stateMachine.AddAutonomicTransition(fallToNeutral);
        _stateMachine.AddAutonomicTransition(dashToNeutral);

        #region OnEnter
        move.OnEnter.AddListener(() =>
        {
            setContextState(CharacterAction.Move);
            setVerticalVelocity(0);
            setHorizontalVelocity(_context.HorizontalMovableSpeed);
            constraintRbAxisY(true);
        });

        idle.OnEnter.AddListener(() =>
        {
            setVerticalVelocity(0);
            setHorizontalVelocity(0);
            setContextState(CharacterAction.Idle);
        });

        jump.OnEnter.AddListener(() =>
        {
            setContextState(CharacterAction.Jump);
            constraintRbAxisY(false);
            setVerticalVelocity(calculateJumpVelocity());
            setHorizontalVelocity(_context.AirborneMovementSpeed);
        });

        fall.OnEnter.AddListener(() =>
        {
            setContextState(CharacterAction.Fall);
            constraintRbAxisY(false);
            setHorizontalVelocity(_context.AirborneMovementSpeed);
        });

        neutral.OnEnter.AddListener(() =>
        {
            setContextState(CharacterAction.Neutral);
        });

        dash.OnEnter.AddListener(() =>
        {
            setContextState(CharacterAction.Dash);
            constraintRbAxisY(true);
            resetDash();
            dashing();
        });
        #endregion

        #region Update
        idle.OnUpdate.AddListener(() =>
        {
            setCharacterOrientator();
            blendHorizontalVelocity();
            applyRbMovement();
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

        dash.OnUpdate.AddListener(() =>
        {
            handleDashing();
        });
        #endregion

        #region OnExit
        dash.OnExit.AddListener(() =>
        {
            resetDash();
        });
        #endregion

        _stateMachine.SetState(CharacterAction.Idle);

        setContextGravity();
    }

    private void setHasDashEnded(bool hasEnded)
    {
        _context.HasDashEnded = hasEnded;
    }

    private void resetDash()
    {
        _context.CurrentDashDuration = _context.DashDuration;
        setHasDashEnded(false);
    }

    public void HandleInput(InputSignal inputSignal)
    {
        var dir = inputSignal.Value;
        _context.MoveInput = dir != null ? (Vector2)dir : Vector2.zero;
        _stateMachine.SetState(inputSignal.Action);
    }

    private void setContextState(CharacterAction actionType)
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
        _context.Rb.linearVelocity = new Vector3(_context.DashSpeed * _context.LastFaceX, 0, 0);
        _context.VerticalVelocity = 0;
    }

    private void setHorizontalVelocity(float velocity)
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
    }

    private bool isGrounded()
    {
        foreach (var checkPoint in _context.GroundCheckPoints)
        {
            DrawDebugCircle(checkPoint.position, _context.GroundCheckDistance, Color.green);

            if (Physics2D.OverlapCircle(checkPoint.position, _context.GroundCheckDistance, _context.PlatformLayer))
            {
                return true;
            }
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

    private void debuffJumpingRise()
    {
        var sign = Mathf.Sign(_context.VerticalVelocity);
        _context.VerticalVelocity = (sign > 0) ? _context.VerticalVelocity / 1.5f : _context.VerticalVelocity;
    }

    private void handleDashing()
    {
        _context.CurrentDashDuration -= Time.deltaTime;
        if (_context.CurrentDashDuration < 0)
        {
            setHasDashEnded(true);
        }
    }

    [System.Serializable]
    private class Context
    {
        public CharacterAction CurrentAction;
        [HideInInspector] public float Gravity;
        [HideInInspector] public Rigidbody2D Rb;
        public Vector2 MoveInput;
        [HideInInspector] public Transform MoverTransform;
        [HideInInspector] public Transform OrientationTransform;
        [HideInInspector] public Transform[] GroundCheckPoints;
        public LayerMask PlatformLayer;
        public float GroundCheckDistance;

        public float HorizontalMovableSpeed;
        [HideInInspector] public float HorizontalCurrentVelocity;
        public float Acceleration;

        public float DashSpeed;

        [HideInInspector] public int LastFaceX;
        [HideInInspector] public float VerticalVelocity;
        [HideInInspector] public float HorizontalVelocity;
        public float JumpTimeToPeak;
        public float FaceDeadzone;
        public float JumpHeight;
        public float AirborneMovementSpeed;
        public bool IsGrounded;

        [HideInInspector] public bool HasDashEnded = false;
        public float DashDuration;
        [HideInInspector] public float CurrentDashDuration;
    }
}