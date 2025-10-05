using DevVorpian;
using R3;
using UnityEngine;

[System.Serializable]
public class AutomaticGunCombat : MonoBehaviour, ICombat
{
    private StateMachine<CharacterAction> _stateMachine;
    [SerializeField] private Context _context;

    private CompositeDisposable _disposables = new();

    public void Init(Subject<Unit> transitionStream)
    {
        var idle = new ConcreteState("Idle");
        var fire = new ConcreteState("Fire");
        var reload = new ConcreteState("Reload");
        var neutral = new ConcreteState("Neutral");

        var toIdle = new StateTransition<CharacterAction>(null, idle, CharacterAction.Idle, condition: () => _context.CurrentAction != CharacterAction.Reload);
        var toFire = new StateTransition<CharacterAction>(null, fire, CharacterAction.Attack, condition: () => _context.CurrentCharge > 0 && _context.CurrentAction != CharacterAction.Reload);
        var toReload = new StateTransition<CharacterAction>(null, reload, CharacterAction.Reload, condition: () => _context.CurrentCharge < _context.ChargeCapacity && _context.CurrentAction != CharacterAction.Reload, onTransition: () => Debug.Log("To Reload"));

        var fireToReload = new StateTransition<CharacterAction>(fire, reload, CharacterAction.Reload, condition: () => _context.CurrentCharge < 1);
        var reloadToNeutral = new StateTransition<CharacterAction>(reload, neutral, CharacterAction.Neutral, condition: () => _context.IsReloaded, onTransition: () => Debug.Log("ToNeutral"));
        var neutralToIdle = new StateTransition<CharacterAction>(neutral, idle, CharacterAction.Idle, () => !_context.IsAttackRequested);
        var neutralToFire = new StateTransition<CharacterAction>(neutral, fire, CharacterAction.Attack, () => _context.IsAttackRequested);

        _stateMachine = new StateMachine<CharacterAction>();

        _stateMachine.AddIntentBasedTransition(toFire);
        _stateMachine.AddIntentBasedTransition(toIdle);
        _stateMachine.AddIntentBasedTransition(toReload);

        _stateMachine.AddAutonomicTransition(fireToReload);
        _stateMachine.AddAutonomicTransition(reloadToNeutral);
        _stateMachine.AddAutonomicTransition(neutralToFire);
        _stateMachine.AddAutonomicTransition(neutralToIdle);

        _stateMachine.OnTransitionedAutonomously.AddListener(() => transitionStream.OnNext(Unit.Default));

        #region OnEnter
        idle.OnEnter.AddListener(() =>
        {
            setContextState(CharacterAction.Idle);
        });

        fire.OnEnter.AddListener(() =>
        {
            setContextState(CharacterAction.Attack);
        });

        reload.OnEnter.AddListener(() =>
        {
            setIsReloaded(false);
            playReloadSound();
            setContextState(CharacterAction.Reload);
        });

        neutral.OnEnter.AddListener(() =>
        {
            setContextState(CharacterAction.Neutral);
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

        _stateMachine.SetState(CharacterAction.Idle);

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
            case CharacterAction.Idle:
                setAttackRequest(false);
                break;
            case CharacterAction.Attack:
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
    }

    private void forceToNeutral()
    {
        _stateMachine.SetState(CharacterAction.Neutral);
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

    private void setContextState(CharacterAction action)
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
        public CharacterAction CurrentAction;
        public float LastFireTime;
        public bool IsReloaded;
        public int CurrentCharge;
        public float CurrentReloadProgress;
        #endregion
    }
}
