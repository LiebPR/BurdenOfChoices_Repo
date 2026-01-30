using System;
using UnityEngine;
using UnityEngine.UI;

public class NPCEndSequenceStep : MonoBehaviour, ISequenceStep
{
    [SerializeField] GameObject npc;         // Bibbo
    [SerializeField] Image uiImageToShow;    // Imagen en UI del jugador

    public void Play(Action onFinished)
    {
        if (npc != null)
            Destroy(npc);

        if (uiImageToShow != null)
            uiImageToShow.gameObject.SetActive(true);

        onFinished?.Invoke();
    }
}
