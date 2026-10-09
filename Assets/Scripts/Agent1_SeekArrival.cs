using UnityEngine;
using UnityEngine.InputSystem;

public class Agent1_SeekArrival : SteeringAgentBase
{
    [Header("Arrival Settings")]
    public Transform target;
    public float slowingRadius = 4f;      
    public float stoppingDistance = 1.2f; 

    private void Awake()
    {
        activationKey = Key.Digit1; // Tecla 1 del nuevo sistema
    }

    protected override Vector2 GetBehaviorVelocity(Vector2 currentPos)
    {
        if (target == null) return Vector2.zero;

        Vector2 targetPos = target.position;
        Vector2 toTarget = targetPos - currentPos; 
        float distance = toTarget.magnitude; 

        if (distance <= stoppingDistance)
        {
            return Vector2.zero; 
        }
        else if (distance < slowingRadius)
        {
            float factor = (distance - stoppingDistance) / (slowingRadius - stoppingDistance); 
            return toTarget.normalized * maxVelocity * factor; 
        }

        return CalculateSeek(currentPos, targetPos); 
    }
}