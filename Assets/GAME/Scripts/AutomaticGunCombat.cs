using DevVorpian;
using R3;
using UnityEngine;

[System.Serializable]
public class AutomaticGunCombat : MonoBehaviour, ICombat
{
    private StateMachine<PlayerAction> _stateMachine;
    [SerializeField] private Context _context;

    private InputSignal _cachedInputSignal;
    private Subject<Unit> _transitionStream = new();
    private CompositeDisposable _disposables = new();

    public void Init(Subject<Unit> transitionStream)
    {
        var idle = new ConcreteState("Idle");
        var fire = new ConcreteState("Fire");
        var reload = new ConcreteState("Reload");
        var neutral = new ConcreteState("Neutral");

        var toIdle = new StateTransition<PlayerAction>(null, idle, PlayerAction.Idle, condition: () => _context.CurrentAction != PlayerAction.Reload);
        var toFire = new StateTransition<PlayerAction>(null, fire, PlayerAction.Attack, condition: () => _context.CurrentCharge > 0 && _context.CurrentAction != PlayerAction.Reload);
        var toReload = new StateTransition<PlayerAction>(null, reload, PlayerAction.Reload, condition: () => _context.CurrentCharge < _context.ChargeCapacity && _context.CurrentAction != PlayerAction.Reload, onTransition: () => Debug.Log("To Reload"));

        var fireToReload = new StateTransition<PlayerAction>(fire, reload, PlayerAction.Reload, condition: () => _context.CurrentCharge < 1);
        var reloadToNeutral = new StateTransition<PlayerAction>(reload, neutral, PlayerAction.Neutral, condition: () => _context.IsReloaded, onTransition: () => Debug.Log("ToNeutral"));
        var neutralToIdle = new StateTransition<PlayerAction>(neutral, idle, PlayerAction.Idle, () => !_context.IsAttackRequested);
        var neutralToFire = new StateTransition<PlayerAction>(neutral, fire, PlayerAction.Attack, () => _context.IsAttackRequested);

        _stateMachine = new StateMachine<PlayerAction>();

        _stateMachine.AddIntentBasedTransition(toFire);
        _stateMachine.AddIntentBasedTransition(toIdle);
        _stateMachine.AddIntentBasedTransition(toReload);

        _stateMachine.AddAutonomicTransition(fireToReload);
        _stateMachine.AddAutonomicTransition(reloadToNeutral);
        _stateMachine.AddAutonomicTransition(neutralToFire);
        _stateMachine.AddAutonomicTransition(neutralToIdle);

        _stateMachine.OnTransitionedAutonomously.AddListener(submitAutonomicStateTransition);
        _transitionStream.Subscribe((Unit) => transitionStream.OnNext(Unit.Default)).AddTo(_disposables);


        #region OnEnter
        idle.OnEnter.AddListener(() =>
        {
            setContextState(PlayerAction.Idle);
        });

        fire.OnEnter.AddListener(() =>
        {
            setContextState(PlayerAction.Attack);
        });

        reload.OnEnter.AddListener(() =>
        {
            setIsReloaded(false);
            playReloadSound();
            setContextState(PlayerAction.Reload);
        });

        neutral.OnEnter.AddListener(() =>
        {
            setContextState(PlayerAction.Neutral);
        });
        #endregion

        #region OnUpdate
        fire.OnUpdate.AddListener(() =>
        {
            handleFiring();
        });

        reload.OnUpdate.AddListener(() =>
        {
            handleReloading();
        });
        #endregion

        #region OnExit
        reload.OnExit.AddListener(() =>
        {
            resetReloadStateVariables();
        });
        #endregion

        _stateMachine.SetState(PlayerAction.Idle);

        setCharge(_context.ChargeCapacity);
        resetReloadStateVariables();
    }

    public void Update()
    {
        takeAim(_context.MousePoint);
        _stateMachine?.Update();
    }

    public void HandleInput(InputSignal inputSignal)
    {
        switch (inputSignal.Action)
        {
            case PlayerAction.Idle:
                setAttackRequest(false);
                break;
            case PlayerAction.Attack:
                setAttackRequest(true);
                break;
            default:
                break;
        }
        if (inputSignal.Value != null)
        {
            var mousePoint = (Vector2)inputSignal.Value;
            _context.MousePoint = (mousePoint != null) ? mousePoint : _context.MousePoint;
        }

        _stateMachine.SetState(inputSignal.Action);

        _cachedInputSignal = (inputSignal.Action != PlayerAction.MouseDrag) ? inputSignal : _cachedInputSignal;
    }

    private void forceToNeutral()
    {
        _stateMachine.SetState(PlayerAction.Neutral);
    }

    public void End()
    {
    }

    private void handleFiring()
    {
        if (_context.CurrentCharge <= 0)
        {
            //Debug.Log("Out of Ammo");
        }

        if (Time.time - _context.LastFireTime < _context.ReattackTime)
        {
            return;
        }

        _context.Bullet.Fire(_context.FirePoint.position, _context.FirePoint.right, float.MaxValue);
        _context.LastFireTime = Time.time;
        _context.CurrentCharge--;
    }

    private void setContextState(PlayerAction action)
    {
        _context.CurrentAction = action;
    }

    private void setCharge(int amount)
    {
        _context.CurrentCharge = amount;
    }

    private void takeAim(Vector2 aimPoint)
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(aimPoint);
        mouseWorldPos.z = 0;

        Vector3 toTarget = (mouseWorldPos - _context.FirePoint.position);
        if (toTarget.magnitude < _context.MinAimDistance)
            return;

        Vector3 direction = toTarget.normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);

        if (direction.x < 0)
        {
            rot *= Quaternion.Euler(180f, 0f, 0f);
        }

        _context.RifleTransform.rotation = rot;
    }

    private void setIsReloaded(bool isReloaded)
    {
        _context.IsReloaded = isReloaded;
    }

    private void handleReloading()
    {
        _context.CurrentReloadProgress -= Time.deltaTime;
        if (_context.CurrentReloadProgress <= 0)
        {
            setCharge(_context.ChargeCapacity);
            setIsReloaded(true);
        }
    }

    private void resetReloadStateVariables()
    {
        _context.CurrentReloadProgress = _context.ReloadTime;
        setIsReloaded(false);
    }

    private void playReloadSound()
    {
        SoundManager.Instance.CreateSoundBuilder()
        .WithRandomPitch(-0.25f, 0.25f)
        .Play(_context.ReloadSound);
    }

    private void submitAutonomicStateTransition()
    {
        _transitionStream.OnNext(Unit.Default);
    }

    private void setAttackRequest(bool isAttackRequested)
    {
        _context.IsAttackRequested = isAttackRequested;
    }

    [System.Serializable]
    public class Context
    {
        public Vector2 MousePoint;
        public SpriteRenderer RifleSprite;
        public Transform RifleTransform;
        public Transform FirePoint;
        public int ChargeCapacity;
        public float ReattackTime;
        public float ReloadTime;
        public float MinAimDistance;
        public bool IsAttackRequested;
        public HitscanBullet Bullet;
        public SoundData ReloadSound;

        #region Non Predeterministic Variables
        [Header("No predeterministics")]
        public PlayerAction CurrentAction;
        public float LastFireTime;
        public bool IsReloaded;
        public int CurrentCharge;
        public float CurrentReloadProgress;
        #endregion
    }
}
