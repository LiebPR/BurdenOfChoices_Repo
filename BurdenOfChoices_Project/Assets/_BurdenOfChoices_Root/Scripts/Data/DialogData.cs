using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Contenedor de datos puro para diálogos. 
/// No contiene lógica.
/// </summary>
[CreateAssetMenu(menuName = "Dialog/DialogData")]
public class DialogData : ScriptableObject
{
    [Header("Speaker")]
    [Tooltip("Nombre del parlante.")]
    public string speakerName;

    [Header("Lines")]
    [Tooltip("Líneas del diálogo en orden.")]
    public List<string> lines = new List<string>();

    [Header("Emotions")]
    public List<Emotion> emotions = new List<Emotion>();

    [Header("Settings")]
    [Tooltip("Avance automático del diálogo.")]
    public bool autoAdvance;
    public float typeSpeed = 0.03f;

    [Tooltip("Tiempo entre líneas si el avance es automático.")]
    public float autoAdvanceDelay = 2f;

    [Tooltip("Bloquea el control del jugador mientras el diálogo está activo.")]
    public bool blockPlayerController = true;

    [Header("Audio")]
    public string typingSFXID = "Dialog_";
    public float typingSFXInterval = 0.05f;
    public float typingVolume = 0.5f;
}

//Define una emoción y el sprite asociado.
[System.Serializable]
public class Emotion
{
    public string name; 
    public Sprite portrait; //Imagen que muestra en la UI
}
