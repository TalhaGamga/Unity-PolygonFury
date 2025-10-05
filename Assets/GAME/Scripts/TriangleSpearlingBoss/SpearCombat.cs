using DevVorpian;
using R3;
using UnityEngine;

public class SpearCombat :MonoBehaviour, ICombat
{
    private StateMachine<CharacterAction> _stateMachine;
    [SerializeField] private Context _context;

    public void Init(Subject<Unit> transitionStream)
    {
        var idle = new ConcreteState("Idle");
        var attack = new ConcreteState("Attack");

        var toIdle = new StateTransition<CharacterAction>(null, idle, CharacterAction.Idle);
        var toAttack = new StateTransition<CharacterAction>(null, attack, CharacterAction.Attack);

        _stateMachine = new StateMachine<CharacterAction>();

        _stateMachine.AddIntentBasedTransition(toIdle);
        _stateMachine.AddIntentBasedTransition(toAttack);
    }

    public void HandleInput(InputSignal inputSignal)
    {
        throw new System.NotImplementedException();
    }

    public void Update()
    {
        throw new System.NotImplementedException();
    }

    public void End()
    {
        throw new System.NotImplementedException();
    }

    [System.Serializable]
    public class Context
    {
        public CharacterAction CurrentAction;
        public Transform SpearTransform;
    }
}