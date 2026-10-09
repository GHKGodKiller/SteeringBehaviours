using UnityEngine;
using UnityEngine.InputSystem;

public class Agent2_Flee : SteeringAgentBase
{
    private Camera mainCam;

    private void Awake()
    {
        activationKey = Key.Digit2; // Tecla 2
        mainCam = Camera.main;
    }

    protected override Vector2 GetBehaviorVelocity(Vector2 currentPos)
    {
        if (mainCam == null || Mouse.current == null) return Vector2.zero;

        // Lectura de la posición del mouse usando el nuevo Input System
        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
        Vector2 mousePosition = mainCam.ScreenToWorldPoint(mouseScreenPosition);
        
        return -CalculateSeek(currentPos, mousePosition); 
    }
}