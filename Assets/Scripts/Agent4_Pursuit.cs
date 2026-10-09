using UnityEngine;
using UnityEngine.InputSystem;

public class Agent4_Pursuit : SteeringAgentBase
{
    [Header("Pursuit Target")]
    public SteeringAgentBase targetAgent; 

    private void Awake()
    {
        activationKey = Key.Digit4; // Tecla 4
    }

    protected override Vector2 GetBehaviorVelocity(Vector2 currentPos)
    {
        if (targetAgent == null) return Vector2.zero;

        Vector2 targetPos = targetAgent.transform.position;
        Vector2 toTarget = targetPos - currentPos;

        float lookAheadTime = toTarget.magnitude / maxVelocity;
        Vector2 futurePosition = targetPos + targetAgent.Velocity * lookAheadTime;

        return CalculateSeek(currentPos, futurePosition);
    }
}