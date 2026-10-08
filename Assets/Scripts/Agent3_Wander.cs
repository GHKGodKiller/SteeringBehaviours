using UnityEngine;
using UnityEngine.InputSystem;

public class Agent3_Wander : SteeringAgentBase
{
    [Header("Wander Settings")]
    public float wanderRadius = 2f;      
    public float wanderDistance = 3f;    
    public float wanderJitter = 50f;     
    
    private float wanderAngle;

    protected override void Awake()
    {
        base.Awake();
        activationKey = Key.Digit3; 
    }

    protected override Vector2 GetBehaviorVelocity(Vector2 currentPos)
    {
        wanderAngle += Random.Range(-1f, 1f) * wanderJitter * Time.fixedDeltaTime; 

        Vector2 forward = velocity.sqrMagnitude > 0.001f ? velocity.normalized : (Vector2)transform.up;
        Vector2 circleCenter = currentPos + forward * wanderDistance; 
        
        Vector2 displacement = new Vector2(Mathf.Cos(wanderAngle), Mathf.Sin(wanderAngle)) * wanderRadius; 
        Vector2 wanderTarget = circleCenter + displacement; 

        return CalculateSeek(currentPos, wanderTarget); 
    }
}