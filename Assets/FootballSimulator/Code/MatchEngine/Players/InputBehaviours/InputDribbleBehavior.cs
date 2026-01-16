using FStudio.MatchEngine.Players.Behaviours;
using FStudio.MatchEngine.Players.InputBehaviours;
using UnityEngine;

public class InputDribbleBehavior : BaseBehaviour, IInputBehaviour
{
    public bool IsTriggered { private get; set; }
    public Vector3 InputDirection { set; private get; }

    public override bool Behave(bool isAlreadyActive)
    {
        if (!IsTriggered && !isAlreadyActive)
        {
            return false;
        }

        // if AI
        if (!Player.isInputControlled)
        {
            IsTriggered = false;
            return false;
        }

        if (ball)
        {
            Player.DoTackle(ball, forceTackle: true);
            Debug.Log("[InputDribbleBehavior] Input pressed");
        }

        return false;
    }
}