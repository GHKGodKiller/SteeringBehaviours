using UnityEngine;
using UnityEngine.InputSystem; // Requerido para el nuevo Input System

public abstract class SteeringAgentBase : MonoBehaviour
{
    [Header("Configuración Base")]
    public float maxVelocity = 5f; 
    public float maxForce = 5f;    
    public float mass = 1f;        
    
    [Header("Obstacle Avoidance")]
    public float maxSeeAhead = 4f;        
    public float obstacleRadius = 1.5f;   
    public Transform[] customObstacles;  

    protected Key activationKey; // Usando el tipo Key del nuevo Input System
    protected Vector2 velocity;
    private bool isActive = false;

    public Vector2 Velocity => velocity;

    protected virtual void Update()
    {
        // Validación y lectura directa del teclado con el Nuevo Input System
        if (Keyboard.current != null && Keyboard.current[activationKey].wasPressedThisFrame)
        {
            isActive = !isActive;
            if (!isActive) velocity = Vector2.zero; // Detiene el comportamiento
        }

        if (!isActive) return;

        Vector2 position = transform.position; 
        Vector2 desiredVelocity = GetBehaviorVelocity(position); 

        Vector2 steering = desiredVelocity - velocity; 

        Vector2 avoidanceForce = CalculateObstacleAvoidance(position);
        steering += avoidanceForce; 

        steering = Vector2.ClampMagnitude(steering, maxForce); 
        Vector2 acceleration = steering / mass;                

        velocity = Vector2.ClampMagnitude(velocity + (acceleration * Time.deltaTime), maxVelocity); 
        transform.position += (Vector3)(velocity * Time.deltaTime);                                  

        RotateTowardsVelocity();
    }

    protected abstract Vector2 GetBehaviorVelocity(Vector2 currentPos);

    protected Vector2 CalculateSeek(Vector2 currentPos, Vector2 targetPos)
    {
        Vector2 desired = targetPos - currentPos; 
        return desired.magnitude > 0.001f ? desired.normalized * maxVelocity : Vector2.zero; 
    }

    private Vector2 CalculateObstacleAvoidance(Vector2 currentPos)
    {
        if (customObstacles == null || customObstacles.Length == 0) return Vector2.zero;

        Vector2 forward = velocity.sqrMagnitude > 0.001f ? velocity.normalized : (Vector2)transform.up;
        float dynamicSeeAhead = (velocity.magnitude / maxVelocity) * maxSeeAhead; 
        
        Vector2 ahead = currentPos + forward * dynamicSeeAhead; 
        Vector2 ahead2 = currentPos + forward * (dynamicSeeAhead * 0.5f); 

        Vector2 mostThreateningPos = Vector2.zero;
        bool threatFound = false;
        float minDistance = float.MaxValue;

        foreach (Transform obstacle in customObstacles)
        {
            if (obstacle == null) continue;
            Vector2 obsPos = obstacle.position;

            bool isInsideAhead = Vector2.Distance(ahead, obsPos) <= obstacleRadius;      
            bool isInsideAhead2 = Vector2.Distance(ahead2, obsPos) <= obstacleRadius;    
            bool isInsideCenter = Vector2.Distance(currentPos, obsPos) <= obstacleRadius; 

            if (isInsideAhead || isInsideAhead2 || isInsideCenter)
            {
                float dist = Vector2.Distance(currentPos, obsPos); 
                if (dist < minDistance)
                {
                    minDistance = dist; 
                    mostThreateningPos = obsPos;
                    threatFound = true;
                }
            }
        }

        if (threatFound)
        {
            Vector2 avoidanceForce = ahead - mostThreateningPos; 
            return avoidanceForce.normalized * maxForce; 
        }

        return Vector2.zero;
    }

    private void RotateTowardsVelocity()
    {
        if (velocity.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg; 
            transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
        }
    }
}