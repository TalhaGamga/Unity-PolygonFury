using System.Collections.Generic;
using System;
using UnityEngine.Events;
using UnityEngine;

namespace DevVorpian
{
    [System.Serializable]
    public class StateMachine<StateType>
    {
        public UnityEvent OnTransitionedAutonomously = new();
        public string CurrentStateName => _currentState.StateName;

        private List<StateTransition<StateType>> _intentBasedTransitions;
        private List<StateTransition<StateType>> _autonomicTransitions;

        private ConcreteState _currentState;

        public StateMachine()
        {
            _intentBasedTransitions = new();
            _autonomicTransitions = new();
            _currentState = new ConcreteState();
        }

        public void Update()
        {
            _currentState?.Update();
            checkTransition();
        }

        public void AddIntentBasedTransition(StateTransition<StateType> stateTransition)
        {
            _intentBasedTransitions.Add(stateTransition);
        }

        public void AddAutonomicTransition(StateTransition<StateType> stateTransition)
        {
            _autonomicTransitions.Add(stateTransition);
        }

        public void SetState(StateType newStateType)
        {
            var transitionData = findInputBasedTransition(newStateType);
            if (transitionData == null)
            {
                return;
            }

            setState(transitionData);
        }

        public void AddAnyTransitionTrigger(ref Action action, StateTransition<StateType> transition)
        {
            action += () =>
            {
                if (transition.Condition != null && !transition.Condition())
                {
                    return;
                }

                if (!_currentState.Equals(transition.To))
                {
                    var transitionData = new StateTransitionData(transition.To, transition.OnTransition);
                    setState(transitionData);
                }
            };
        }

        public void AddNormalTransitionTrigger(ref Action action, StateTransition<StateType> transition)
        {
            action += () =>
            {
                Debug.Log("Normal Transition Triggered: " + transition.TargetStateType);
                if (transition.Condition != null && !transition.Condition())
                {
                    return;
                }

                if (_currentState.Equals(transition.From) && !_currentState.Equals(transition.To))
                {
                    var transitionData = new StateTransitionData(transition.To, transition.OnTransition);
                    setState(transitionData);
                }
            };
        }

        private void setStateAutonomous(StateTransitionData transitionData)
        {
            if (transitionData != null)
            {
                setState(transitionData);
                OnTransitionedAutonomously?.Invoke();
            }
        }

        private void checkTransition()
        {
            StateTransitionData transitionData = findAutonomicTransition();
            setStateAutonomous(transitionData);
        }

        private void setState(StateTransitionData transitionData)
        {
            _currentState?.Exit();
            transitionData?.OnTransition();
            _currentState = transitionData.TargetState;
            _currentState.Enter();
        }

        private StateTransitionData findInputBasedTransition(StateType targetStateType)
        {
            foreach (var t in _intentBasedTransitions)
            {
                if (!t.TargetStateType.Equals(targetStateType)) continue;
                if (t.From != null && t.From.Equals(_currentState) && t.Condition())
                    return new StateTransitionData(t.To, t.OnTransition);
            }

            foreach (var t in _intentBasedTransitions)
            {
                if (!t.TargetStateType.Equals(targetStateType)) continue;
                if (t.From == null && !_currentState.Equals(t.To) && t.Condition())
                    return new StateTransitionData(t.To, t.OnTransition);
            }

            return null;
        }

        private StateTransitionData findAutonomicTransition()
        {
            var current = _currentState;
            StateTransition<StateType> fallback = null;

            foreach (var transition in _autonomicTransitions)
            {
                if (current.Equals(transition.To))
                    continue;

                if (!transition.Condition())
                    continue;

                if (transition.From != null && transition.From.Equals(current))
                    return new StateTransitionData(transition.To, transition.OnTransition);

                if (transition.From == null && fallback == null)
                    fallback = transition;
            }

            return fallback != null
                ? new StateTransitionData(fallback.To, fallback.OnTransition)
                : null;
        }
    }

    public class StateTransition<ActionType>
    {
        private ConcreteState _from;
        private ConcreteState _to;
        private ActionType _targetStateType;
        private Func<bool> _condition;
        private Action _onTransition;
        private int _priority { get; set; }

        public StateTransition(
            ConcreteState from,
            ConcreteState to,
            ActionType targetStateType,
            Func<bool> condition = null,
            Action onTransition = null)
        {
            _from = from;
            _to = to;
            _targetStateType = targetStateType;
            _condition = condition ?? (() => true);
            _onTransition = onTransition ?? (() => { });
        }

        public ConcreteState From => _from;
        public ConcreteState To => _to;
        public ActionType TargetStateType => _targetStateType;
        public Func<bool> Condition => _condition;
        public Action OnTransition => _onTransition;
        public int Priority => _priority;
    }

    public class StateTransitionData
    {
        public ConcreteState TargetState;
        public Action OnTransition;
        public StateTransitionData(ConcreteState state, Action onTransition)
        {
            TargetState = state;
            OnTransition = onTransition;
        }
    }
}