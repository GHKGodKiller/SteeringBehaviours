using UnityEngine;
using UnityEngine.InputSystem;

public class Agent5_Evade : SteeringAgentBase
{
    [Header("Evade Target")]
    public SteeringAgentBase targetAgent; 

    protected override void Awake()
    {
        base.Awake();
        activationKey = Key.Digit5; 
    }

    protected override Vector2 GetBehaviorVelocity(Vector2 currentPos)
    {
        if (targetAgent == null) return Vector2.zero;

        Vector2 targetPos = targetAgent.transform.position;
        Vector2 toTarget = targetPos - currentPos;

        float lookAheadTime = toTarget.magnitude / maxVelocity;
        Vector2 futurePosition = targetPos + targetAgent.Velocity * lookAheadTime;

        return -CalculateSeek(currentPos, futurePosition);
    }
}