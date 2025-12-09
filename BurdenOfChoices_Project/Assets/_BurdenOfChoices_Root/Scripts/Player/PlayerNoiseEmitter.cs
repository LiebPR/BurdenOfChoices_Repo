using UnityEngine;

public class PlayerNoiseEmitter : MonoBehaviour
{
    public enum MovementState { Running, Walking, Crouched };

    //Estos valores los actualizas desde tu PlayerMovement
    public MovementState currentState;
    public float currentSpeed;

    //Ruido base según el estado del jugador
    float GetBaseNoise()
    {
        switch (currentState)
        {
            case MovementState.Running: return 1.0f; //muy ruidoso
            case MovementState.Walking: return 0.4f; //moderado
            case MovementState.Crouched: return 0f; //silencioso
        }
        return 0f;
    }

    //Ruido final aplicando velocidad
    public float CurrentNoise()
    {
        return GetBaseNoise() * currentSpeed;
    }
}
