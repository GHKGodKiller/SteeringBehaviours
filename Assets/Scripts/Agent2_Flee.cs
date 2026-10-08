using UnityEngine;
using UnityEngine.InputSystem;

public class Agent2_Flee : SteeringAgentBase
{
    private Camera mainCam;

    protected override void Awake()
    {
        base.Awake();
        activationKey = Key.Digit2; 
        mainCam = Camera.main;
    }

    protected override Vector2 GetBehaviorVelocity(Vector2 currentPos)
    {
        if (mainCam == null || Mouse.current == null) return Vector2.zero;

        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
        Vector2 mousePosition = mainCam.ScreenToWorldPoint(mouseScreenPosition);
        
        return -CalculateSeek(currentPos, mousePosition); 
    }
}