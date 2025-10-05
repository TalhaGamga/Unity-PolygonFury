using DevVorpian;
using R3;
using UnityEngine;

namespace TriangleSpearlingBoss
{

    [System.Serializable]
    public class BossMover : IMover
    {
        private StateMachine<CharacterAction> _stateMachine;
        [SerializeField] private Context _context;

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

            var toIdle = new StateTransition<CharacterAction>(null, idle, CharacterAction.Idle, () => _context.MoveInput.magnitude == 0 && _context.IsGrounded);
            var toMove = new StateTransition<CharacterAction>(null, move, CharacterAction.Move, () => _context.MoveInput.magnitude > 0 && _context.IsGrounded);

            _stateMachine = new StateMachine<CharacterAction>();

            _stateMachine.OnTransitionedAutonomously.AddListener(() => transitionStream.OnNext(Unit.Default));

            _stateMachine.AddIntentBasedTransition(toIdle);
            _stateMachine.AddIntentBasedTransition(toMove);


            #region OnEnter
            idle.OnEnter.AddListener(() =>
            {
                setContextState(CharacterAction.Idle);
            });

            move.OnEnter.AddListener(() =>
            {
                setContextState(CharacterAction.Move);
                setHorizontalVelocity(_context.HorizontalMaxSpeed);
                constraintRbAxisY(true);
            });

            #endregion

            #region OnUpdate
            idle.OnUpdate.AddListener(() =>
            {
                blendHorizontalVelocity();
                applyRbMovement();
            });

            move.OnUpdate.AddListener(() =>
            {
                setCharacterOrientator();
                blendHorizontalVelocity();
            });
            #endregion

            #region OnExit
            #endregion

            _stateMachine.SetState(CharacterAction.Idle);
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

        private void setHorizontalVelocity(float velocity)
        {
            _context.HorizontalTargetVelocity = velocity;
        }

        private void constraintRbAxisY(bool isAllowed)
        {
            _context.Rb.constraints = isAllowed
                ? RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionY
                : RigidbodyConstraints2D.FreezeRotation;
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

        private void blendHorizontalVelocity()
        {
            var desiredSpeed = _context.HorizontalTargetVelocity * _context.MoveInput.x;
            _context.HorizontalCurrentVelocity = Mathf.MoveTowards(_context.HorizontalCurrentVelocity, desiredSpeed, _context.Acceleration);
        }

        private void applyRbMovement()
        {
            _context.Rb.linearVelocity = new Vector2(_context.HorizontalCurrentVelocity, 0);
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

        [System.Serializable]
        private class Context
        {
            public CharacterAction CurrentAction;

            [HideInInspector] public Rigidbody2D Rb;
            [HideInInspector] public Transform MoverTransform;
            [HideInInspector] public Transform OrientationTransform;
            [HideInInspector] public Transform[] GroundCheckPoints;
            [HideInInspector] public float HorizontalTargetVelocity;
            [HideInInspector] public float HorizontalCurrentVelocity;
            [HideInInspector] public int LastFaceX;
            [HideInInspector] public bool IsGrounded;

            public float HorizontalMaxSpeed;
            public float Acceleration;
            public Vector2 MoveInput;
            public LayerMask PlatformLayer;
            public float GroundCheckDistance;
            public float FaceDeadzone;
        }
    }
}
