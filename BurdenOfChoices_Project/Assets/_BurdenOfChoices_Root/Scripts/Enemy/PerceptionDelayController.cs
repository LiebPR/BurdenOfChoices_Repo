using System;
using UnityEngine;

/// <summary>
/// Controla un único delay para cualquier tipo de percepción del enemigo.
/// Gestiona conteo, decaimiento y callback cuando el delay se completa.
/// </summary>
public class PerceptionDelayController : MonoBehaviour
{
    public enum PerceptionType
    {
        None,
        Visual,
        SoundWalk,
        SoundRun,
        Hit,
        Perception,
        Lost
    }

    // Public States
    public EnemyData enemyData;

    #region Internal States
    PerceptionType currentType = PerceptionType.None;
    float counter = 0f;
    bool isCounting;
    bool isStimulusActive;
    Vector3 currentStimulusPosition;
    float lostCounter = 0f;
    bool isLostCounting = false;
    #endregion

    #region Events
    public delegate void OnDelayComplete(PerceptionType type, Vector3 stimulusPos);
    public event OnDelayComplete DelayCompleted;
    #endregion

    void Update()
    {
        // Si no hay conteo activo, no hacemos nada
        if (!isCounting && !isLostCounting) return;

        // Si estamos contando el "Lost", lo descontamos
        if (isLostCounting)
        {
            lostCounter -= Time.deltaTime;
            Debug.Log($"[PerceptionDelay] Lost decay: {lostCounter:F2}s remaining");
            if (lostCounter <= 0f)
            {
                lostCounter = 0f;
                isLostCounting = false;
                DelayCompleted?.Invoke(PerceptionType.Lost, currentStimulusPosition);
            }
        }

        // Descontamos el contador principal solo si es necesario
        if (isCounting && isStimulusActive)
        {
            counter -= Time.deltaTime; // Decae en lugar de contar hacia adelante
            float targetDelay = GetDelay(currentType);
            Debug.Log($"[PerceptionDelay] Counting {currentType}: {counter:F2}s");

            // Si el contador llega a cero, completamos el delay
            if (counter <= 0f)
            {
                counter = 0f;
                CompleteDelay();
            }
        }
    }

    #region API
    // Inicia o continúa un delay según el tipo de percepción
    public void StartOrContinueDelay(PerceptionType type, Vector3 stimulusPos)
    {
        // Si el tipo de percepción ha cambiado, reiniciamos el contador
        if (type != currentType)
        {
            currentType = type;
            counter = GetDelay(currentType); // Establecemos el contador al valor del delay
        }
        currentStimulusPosition = stimulusPos;
        isStimulusActive = true;
        isCounting = true;
    }

    // Establece si el estímulo está activo o no
    public void SetStimulusActive(bool active)
    {
        isStimulusActive = active;
    }

    // Normaliza el progreso del delay
    public float GetNormalizedProgress()
    {
        float delay = GetDelay(currentType);
        if (delay <= 0) return 1f;
        return Mathf.Clamp01((delay - counter) / delay); // Progreso basado en el contador
    }

    // Inicia el delay cuando se pierde el objetivo
    public void StartLostDelay(Vector3 stimulusPos)
    {
        isLostCounting = true;
        lostCounter = GetDelay(PerceptionType.Lost);
        currentStimulusPosition = stimulusPos;
    }
    #endregion

    // Completa el delay y dispara el evento de finalización
    void CompleteDelay()
    {
        isCounting = false;
        DelayCompleted?.Invoke(currentType, currentStimulusPosition);
        counter = 0f;
        currentType = PerceptionType.None;
    }

    // Obtiene el delay correspondiente a un tipo de percepción
    float GetDelay(PerceptionType type)
    {
        switch (type)
        {
            case PerceptionType.Visual: return enemyData.visionDelay;
            case PerceptionType.SoundWalk: return enemyData.hearingDelayWalk;
            case PerceptionType.SoundRun: return enemyData.hearingDelayRun;
            case PerceptionType.Hit: return 0.01f; // Muy rápido
            case PerceptionType.Perception: return enemyData.perceptionDelay;
            case PerceptionType.Lost: return enemyData.lostDelay;
        }
        return enemyData.hearingDelayWalk;
    }
}