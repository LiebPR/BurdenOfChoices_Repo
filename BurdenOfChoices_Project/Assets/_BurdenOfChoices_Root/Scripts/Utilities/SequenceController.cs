using System.Collections.Generic;
using UnityEngine;

public class SequenceController : MonoBehaviour
{
    [SerializeField] List<MonoBehaviour> steps;

    int currentIndex;

    public void Play()
    {
        Debug.Log($"Play llamado en {name}");
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
        while (currentIndex < steps.Count)
        {
            var step = steps[currentIndex] as ISequenceStep;
            currentIndex++;
            if (step != null)
            {
                step.Play(PlayNext);
                break; // espera callback
            }
        }

        if (currentIndex >= steps.Count)
            Debug.Log("SequenceController: Secuencia finalizada");
    }
}
