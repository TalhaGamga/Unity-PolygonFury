using System.Collections.Generic;

public static class InputBehaviorMap
{
    public static readonly Dictionary<CharacterAction, InputBehavior> Behavior = new()
    {
        { CharacterAction.Move, InputBehavior.Stateful },
        { CharacterAction.Jump, InputBehavior.Eventful },
        { CharacterAction.JumpCancel, InputBehavior.Eventful },
        { CharacterAction.Attack, InputBehavior.Eventful },
        {CharacterAction.Dash, InputBehavior.Eventful },
        // Extend...
    };
}