using DevVorpian;
using DG.Tweening;
using R3;
using System;
using UnityEngine;

public class SpearCombat : MonoBehaviour, ICombat
{
    [SerializeField] private StateMachine<CharacterAction> _stateMachine;
    [SerializeField] private Context _context;

    public void Init(Subject<Unit> transitionStream)
    {
        var idle = new ConcreteState("Idle");
        var attack = new ConcreteState("Attack");
        var neutral = new ConcreteState("Neutral");

        var toIdle = new StateTransition<CharacterAction>(null, idle, CharacterAction.Idle);
        var toAttack = new StateTransition<CharacterAction>(idle, attack, CharacterAction.Attack);
        var toNeutral = new StateTransition<CharacterAction>(attack, neutral, CharacterAction.Neutral);
        var neutralToIdle = new StateTransition<CharacterAction>(neutral, idle, CharacterAction.Idle);

        _stateMachine = new StateMachine<CharacterAction>();

        _stateMachine.AddIntentBasedTransition(toIdle);
        _stateMachine.AddIntentBasedTransition(toAttack);

        _stateMachine.AddNormalTransitionTrigger(ref _context.SpearThrownAction, toNeutral);
        _stateMachine.AddNormalTransitionTrigger(ref _context.ReloadAction, neutralToIdle);

        saveSpearTransorm();

        idle.OnEnter.AddListener(() =>
        {
            resetSpear();
            setContextState(CharacterAction.Idle);
        });

        attack.OnEnter.AddListener(() =>
        {
            setSpearAim(setMousePoint(Input.mousePosition));
            setContextState(CharacterAction.Attack);
        });

        neutral.OnEnter.AddListener(() =>
        {
            setContextState(CharacterAction.Neutral);
        });

        _stateMachine.SetState(CharacterAction.Idle);

        Physics2D.IgnoreCollision(_context.CharacterTransform.GetComponent<Collider2D>(), _context.SpearTransform.GetComponent<Collider2D>(), true);
    }

    public void HandleInput(InputSignal inputSignal)
    {
        switch (inputSignal.Action)
        {
            case CharacterAction.Idle:
                break;
            case CharacterAction.Attack:
                _stateMachine.SetState(inputSignal.Action);
                break;
            case CharacterAction.Reload:
                _context.ReloadAction?.Invoke();
                break;
            default:
                break;
        }
    }

    public void Update()
    {
        _stateMachine?.Update();
    }

    public void End()
    {
    }

    private void resetSpear()
    {
        var spear = _context.SpearTransform;
        spear.SetParent(_context.CharacterModelTransform);

        spear.DOLocalMove(_context.InitialPosition, _context.ResetDuration)
            .SetEase(Ease.InOutExpo);

        spear.DOLocalRotateQuaternion(_context.InitialRotation, _context.ResetDuration)
            .SetEase(Ease.InOutExpo);
        _context.Rb.simulated = false;
        constraintRb(true);
        _context.IsStabbed = false;
        _context.Rb.bodyType = RigidbodyType2D.Dynamic;
    }

    private void saveSpearTransorm()
    {
        _context.InitialPosition = _context.SpearTransform.localPosition;
        _context.InitialRotation = _context.SpearTransform.localRotation;
    }

    private void throwSpear(Vector2 position)
    {
        var spear = _context.SpearTransform;
        var rb = _context.Rb;

        Vector2 dir = (position - (Vector2)spear.position).normalized;
        Vector2 pullPos = (Vector2)spear.position - dir * _context.SpearPullDistance;

        spear.DOMove(pullPos, _context.SpearPullDuration)
             .SetEase(Ease.OutSine)
             .OnComplete(() =>
             {
                 constraintRb(false);
                 _context.Rb.simulated = true;
                 _context.SpearTransform.SetParent(null);
                 rb.linearVelocity = Vector2.zero;
                 rb.AddForce(dir * _context.SpearThrowForce, ForceMode2D.Impulse);
                 _context.SpearThrownAction?.Invoke();
             });
    }

    private void setSpearAim(Vector2 targetPoint)
    {
        var spear = _context.SpearTransform;

        Vector2 dir = (targetPoint - (Vector2)spear.position).normalized;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        spear.DORotate(new Vector3(0f, 0f, angle), _context.AimSetDuration)
             .SetEase(Ease.OutSine)
             .OnComplete(() =>
             {
                 throwSpear(targetPoint);
             });
    }

    private Vector2 setMousePoint(Vector2 mousePoint)
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePoint);
        mouseWorldPos.z = 0;

        return mouseWorldPos;
    }

    private void constraintRb(bool shouldConstraint)
    {
        _context.Rb.constraints = shouldConstraint ? RigidbodyConstraints2D.FreezeAll : RigidbodyConstraints2D.None;
    }

    private void setContextState(CharacterAction newAction)
    {
        _context.CurrentAction = newAction;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_context.IsStabbed) return;

        if (((1 << collision.gameObject.layer) & _context.TargetLayer) != 0)
        {
            _context.IsStabbed = true;

            _context.Rb.linearVelocity = Vector2.zero;
            _context.Rb.angularVelocity = 0;

            _context.Rb.bodyType = RigidbodyType2D.Kinematic;

            var spear = _context.SpearTransform;
            Vector3 embedDir = spear.right * _context.EmbedDistance;

            spear.DOMove(spear.position + embedDir, _context.EmbedDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    _context.SpearTransform.DOShakePosition(_context.ShakeDuration, _context.ShakeStrength, vibrato: 8, randomness: 45f, snapping: false, fadeOut: true);
                });
        }
    }



    [System.Serializable]
    public class Context
    {
        public CharacterAction CurrentAction;
        public Transform SpearTransform;
        public Transform TargetScanPoint;
        public Transform CharacterModelTransform;
        public Transform CharacterTransform;
        public float TargetScanRadius;
        public float ResetDuration;
        public LayerMask TargetLayer;
        public float AimSetDuration;
        public Rigidbody2D Rb;
        public float SpearPullDistance;
        public float SpearPullDuration;
        public float SpearThrowForce;

        public float EmbedDistance = .1f;
        public float EmbedDuration = .05f;
        public float ShakeStrength = .1f;
        public float ShakeDuration = .15f;
        public bool IsStabbed = false;

        [HideInInspector] public Vector3 InitialPosition;
        [HideInInspector] public Vector3 TargetPoint;
        [HideInInspector] public Quaternion InitialRotation;
        [HideInInspector] public Action SpearThrownAction = delegate { };
        [HideInInspector] public Action ReloadAction = delegate { };
    }
}