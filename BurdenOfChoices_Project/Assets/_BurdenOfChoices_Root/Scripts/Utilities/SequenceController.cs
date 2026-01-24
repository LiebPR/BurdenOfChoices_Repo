using System;
using System.Collections.Generic;
using UnityEngine;

public class SequenceController : MonoBehaviour
{
    [SerializeField] List<MonoBehaviour> steps;

    int currentIndex;

    public void Play()
    {
        if (steps == null || steps.Count == 0)
        {
            Debug.LogWarning("SequenceController: No hay pasos asignados.");
            return;
        }

        currentIndex = 0;
        PlayNext();
    }

    void PlayNext()
    {
        if (currentIndex >= steps.Count)
        {
            Debug.Log("SequenceController: Secuencia finalizada");
            return;
        }

        var step = steps[currentIndex] as ISequenceStep;
        if (step == null)
        {
            Debug.LogWarning($"SequenceController: Paso {currentIndex} no implementa ISequenceStep");
            currentIndex++;
            PlayNext();
            return;
        }

        currentIndex++;
        step.Play(PlayNext);
    }
}
