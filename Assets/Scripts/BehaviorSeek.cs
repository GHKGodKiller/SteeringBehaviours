using System.Collections.Generic;
using UnityEngine;

public class SteeringBehaviors : MonoBehaviour
{
    public enum BehaviorType
    {
        Seek,
        Flee,
        Arrival,
        Wander,
        ObstacleAvoidance
    }

    //Behavior Setup
    public BehaviorType currentBehavior = BehaviorType.Seek;
    public Transform target;
    public float maxVelocity = 5f; 
    public float maxForce = 5f;    
    public float mass = 1f;        

    //Arrival Settings
    public float slowingRadius = 4f;      
    public float stoppingDistance = 1.2f; 

    //Wander Settings
    public float wanderRadius = 2f;      
    public float wanderDistance = 3f;    
    public float wanderJitter = 50f;     

    //Obstacle Avoidance Settings
    public float maxSeeAhead = 4f;        
    public float obstacleRadius = 1.5f;   
    public Transform[] customObstacles;  

    private Vector2 velocity;
    private float wanderAngle;

void Update()
    {
        // Validación: Si el comportamiento requiere un objetivo (target) y no está asignado, omite la ejecución para evitar errores
        if (target == null && (currentBehavior == BehaviorType.Seek || currentBehavior == BehaviorType.Flee || currentBehavior == BehaviorType.Arrival))
        {
            return;
        }

        Vector2 position = transform.position; // Obtiene la posición actual del objeto en el plano 2D
        Vector2 targetPosition = target != null ? (Vector2)target.position : Vector2.zero; // Obtiene la posición del objetivo si existe, si no asigna el origen (0,0)
        Vector2 desiredVelocity = Vector2.zero; // Inicializa el vector de velocidad deseada en cero

        // Selecciona y calcula la velocidad deseada según el comportamiento activo
        switch (currentBehavior)
        {
            case BehaviorType.Seek:
                desiredVelocity = CalculateSeek(position, targetPosition); // Obtiene la velocidad deseada directa hacia el objetivo
                break;

            case BehaviorType.Flee:
                desiredVelocity = CalculateFlee(position, targetPosition); // Obtiene la velocidad deseada en sentido opuesto al objetivo
                break;

            case BehaviorType.Arrival:
                desiredVelocity = CalculateArrival(position, targetPosition); // Obtiene la velocidad deseada desacelerando según la proximidad al objetivo
                break;

            case BehaviorType.Wander:
                desiredVelocity = CalculateWander(position); // Obtiene la velocidad deseada hacia un punto dinámico en el círculo proyectado
                break;

            case BehaviorType.ObstacleAvoidance:
                // Si existe un objetivo navega hacia él; si no, proyecta un destino hacia adelante usando el vector del sensor maxSeeAhead
                Vector2 defaultDestination = target != null ? targetPosition : position + GetForwardVector() * maxSeeAhead;
                desiredVelocity = CalculateSeek(position, defaultDestination); // Calcula la velocidad deseada base hacia el destino determinado
                break;
        }

        // Steering Force: Diferencia vectorial entre la velocidad a la que quiere ir y su velocidad actual (Deseada - Actual)
        Vector2 steering = desiredVelocity - velocity; 

        if (currentBehavior == BehaviorType.ObstacleAvoidance)
        {
            Vector2 avoidanceForce = CalculateObstacleAvoidance(position); // Calcula la fuerza de desvío si hay colisión inminente
            steering += avoidanceForce; // Suma vectorial de la fuerza repulsiva lateral a la fuerza de dirección principal
        }

        steering = Vector2.ClampMagnitude(steering, maxForce); // Limita la magnitud de la fuerza aplicada para que no supere maxForce
        Vector2 acceleration = steering / mass;                // Segunda ley de Newton (F = m * a -> a = F / m): Obtiene la aceleración resultante

        velocity = Vector2.ClampMagnitude(velocity + (acceleration * Time.deltaTime), maxVelocity); // Integración de Euler: Actualiza la velocidad según la aceleración y la limita a maxVelocity
        transform.position += (Vector3)(velocity * Time.deltaTime);                                  // Integración de Euler: Actualiza la posición desplazando el objeto según su velocidad en el tiempo transcurrido

        RotateTowardsVelocity(); // Ajusta la rotación del transform hacia la nueva dirección del vector velocidad
    }

    //Formula Methods

    // Usado por: Seek, Arrival (fuera del área de frenado), Wander (hacia el punto dinámico) y ObstacleAvoidance (hacia el destino base)
    private Vector2 CalculateSeek(Vector2 currentPos, Vector2 targetPos)
    {
        // El vector deseado es la resta de posiciones: (Destino - Origen)
        Vector2 desired = targetPos - currentPos; 

        // Si la distancia es casi cero, retorna cero; de lo contrario, normaliza la dirección y multiplica por la velocidad máxima
        return desired.magnitude > 0.001f ? desired.normalized * maxVelocity : Vector2.zero; 
    }

    // Usado por: Flee
    private Vector2 CalculateFlee(Vector2 currentPos, Vector2 targetPos)
    {
        // Invierte el vector de Seek multiplicando por -1 para alejarse del objetivo a máxima velocidad
        return -CalculateSeek(currentPos, targetPos); 
    }

    // Usado por: Arrival
    private Vector2 CalculateArrival(Vector2 currentPos, Vector2 targetPos)
    {
        Vector2 toTarget = targetPos - currentPos; 
        float distance = toTarget.magnitude; // Calcula la distancia escalar hasta el objetivo

        if (distance <= stoppingDistance)
        {
            return Vector2.zero; // Detiene completamente la velocidad al alcanzar la distancia de seguridad
        }
        else if (distance < slowingRadius)
        {
            // Calcula un factor entre 0 y 1 proporcional a la distancia dentro del área de frenado
            float factor = (distance - stoppingDistance) / (slowingRadius - stoppingDistance); 
            
            // Reduce la velocidad progresivamente multiplicando la velocidad máxima por el factor
            return toTarget.normalized * maxVelocity * factor; 
        }

        // Reutiliza Seek cuando el objetivo está fuera del área de desaceleración
        return CalculateSeek(currentPos, targetPos); 
    }

    // Usado por: Wander
    private Vector2 CalculateWander(Vector2 currentPos)
    {
        // Añade una pequeña variación aleatoria al ángulo en cada frame según el jitter
        wanderAngle += Random.Range(-1f, 1f) * wanderJitter * Time.deltaTime; 

        Vector2 forward = GetForwardVector();
        
        // Calcula el centro del círculo proyectando la posición hacia adelante según wanderDistance
        Vector2 circleCenter = currentPos + forward * wanderDistance; 
        
        // Convierte el ángulo polar a un vector cartesiano (Cos para X, Sin para Y) y lo escala por el radio del círculo
        Vector2 displacement = new Vector2(Mathf.Cos(wanderAngle), Mathf.Sin(wanderAngle)) * wanderRadius; 
        
        // El objetivo dinámico es el centro del círculo más el punto proyectado en la circunferencia
        Vector2 wanderTarget = circleCenter + displacement; 

        // Reutiliza Seek navegando hacia el punto aleatorio generado
        return CalculateSeek(currentPos, wanderTarget); 
    }

    // Usado por: ObstacleAvoidance
    private Vector2 CalculateObstacleAvoidance(Vector2 currentPos)
    {
        if (customObstacles == null || customObstacles.Length == 0) return Vector2.zero;

        Vector2 forward = GetForwardVector();
        
        // Escala la longitud del sensor proporcionalmente a la velocidad actual del personaje
        float dynamicSeeAhead = (velocity.magnitude / maxVelocity) * maxSeeAhead; 

        // ahead define qué tan lejos verás: position + normalize(velocity) * MAX_SEE_AHEAD[cite: 1]
        Vector2 ahead = currentPos + forward * dynamicSeeAhead; 
        
        // Punto intermedio del vector sensor para detectar obstáculos a corta distancia
        Vector2 ahead2 = currentPos + forward * (dynamicSeeAhead * 0.5f); 

        Vector2 mostThreateningPos = Vector2.zero;
        bool threatFound = false;
        float minDistance = float.MaxValue;

        foreach (Transform obstacle in customObstacles)
        {
            if (obstacle == null) continue;

            Vector2 obsPos = obstacle.position;

            // Evalúa si el obstáculo interseca con alguno de los puntos proyectados del sensor
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

        Vector2 avoidanceForce = Vector2.zero;

        if (threatFound)
        {
            // Calcula una fuerza de empuje lateral restando el punto proyectado menos la posición del obstáculo
            avoidanceForce = ahead - mostThreateningPos; 
            avoidanceForce = avoidanceForce.normalized * maxForce; 
        }

        return avoidanceForce;
    }


    //Helpers

    private Vector2 GetForwardVector()
    {
        return velocity.sqrMagnitude > 0.001f ? velocity.normalized : (Vector2)transform.up;
    }

    private void RotateTowardsVelocity()
    {
        if (velocity.sqrMagnitude > 0.001f)
        {
            // Obtiene el ángulo de dirección en grados usando la tangente de la velocidad (Y/X)
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg; 
            transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
        }
    }

}