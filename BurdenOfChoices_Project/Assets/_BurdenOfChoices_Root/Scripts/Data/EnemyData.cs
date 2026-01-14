using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "EnemyData", menuName = "EnemyData/Enemy")]
public class EnemyData : ScriptableObject
{

    #region Vision
    [Header("Vision Settings")]
    [Tooltip("Radio maximo de detección visual frontal.")]
    public float visionRadius = 6f;
    [Tooltip("Ángulo del cono de visión en grados.")]
    public float visionAngle = 45f;
    [Tooltip("Radio mínimo de detección inmediata alrededor del enemigo.")]
    public float perceptionRadius = 1f;
    [Tooltip("Capas consideradas como obstáculos visuales")]
    public LayerMask obstacleMask;

    [Header("Vision Delays")]
    [Tooltip("Tiempo necesario para confirmar detección por proximidad.")]
    public float perceptionDelay = 0.5f;
    [Tooltip("Tiempo necesario para confirmar detección visual.")]
    public float visionDelay = 0.2f;
    [Tooltip("Tiempo antes de perder completamente al objetivo.")]
    public float lostDelay = 1f;
    #endregion

    #region Hearing
    [Header("Heraing Settings")]
    [Tooltip("Radio máximo de destección auditiva.")]
    public float maxHearingRadius = 5f;
    [Tooltip("Tiempo durante el cual el enemigo recuerda un sonido")]
    public float noiseMemoryTime = 1f;

    [Header("Heraing Delys")]
    [Tooltip("Retraso para reaccionar a sonidos de caminar.")]
    public float hearingDelayWalk = 1.5f;
    [Tooltip("Retraso para reaccionar a sonidos de carrera.")]
    public float hearingDelayRun = 0.8f;
    #endregion

    #region Sound Investigation
    [Header("Sound Investigation Settings")]
    [Tooltip("Tiempo de duda antes de reaccionar al sonido.")]
    public float soundReactionDelay = 0.6f;
    [Tooltip("Velocidad al investigar un sonido.")]
    public float investigateSpeed = 2.5f;
    [Tooltip("Tiempo que el enemigo inspecciona el punto del sonido.")]
    public float soundInspectTime = 2.5f;
    [Tooltip("Ángulo máximo para considerar alineado al sonido.")]
    public float soundTurnAlignmentAngle = 8f;

    [Header("Sound Rotation Settings")]
    [Tooltip("Fuerza del resorte al girar hacia un sonido.")]
    public float rotationSoundStiffness = 4f;
    [Tooltip("Fricción angular al investigar un sonido.")]
    public float rotationSoundDamping = 14f;
    #endregion

    #region Movement
    [Header("Movement Settings")]
    [Tooltip("Velocidad base durate la patrulla.")]
    public float patrolSpeed = 3f;
    [Tooltip("Velocidad durante la persecución.")]
    public float chaseSpeed = 5f;
    [Tooltip("Velocidad de rotación simple(estados no críticos).")]
    public float rotationSpeed = 8f;
    [Tooltip("Distacia mínima para actualizar el destino del NavMeshAgent.")]
    public float destinationUpdateThreshold = 0.2f;
    #endregion

    #region Chasing Rotation
    [Header("Chasing Setting")]
    [Tooltip("Fuerza del resorte para rotación suave")]
    public float rotationChaseStiffness = 12f;
    [Tooltip("Fricción angular aplicado a la rotación")]
    public float rotationChaseDamping = 20f;
    public float chaseAlignmentAngle = 60f;
    #endregion

    #region Rotation Damping
    [Header("Rotation Damp Settings")]
    [Tooltip("Fuerza del resorte para rotación suave.")]
    public float rotationStiffness = 6f;
    [Tooltip("Nivel de fricción angular aplicada a la rotación.")]
    public float rotationDamping = 10f;
    #endregion

    #region Stop Area
    [Header("Stop Area Settings")]
    [Tooltip("Distancia a la que comienzza el frenado progresivo.")]
    public float stopStartDistance = 6f;
    [Tooltip("Distancia a la que el enemigo se detiene completamente.")]
    public float stopHardDistance = 3f;
    #endregion

    #region Acceleration
    [Header("Acceleration Settings")]
    [Tooltip("Aceleración normal durante movimiento estándar.")]
    public float normalAcceleration = 8f;
    [Tooltip("Aceleración usada para frenadas bruscas.")]
    public float breackAcceleration = 25f;
    #endregion

    #region Idle
    [Header("Idle Settings")]
    [Tooltip("Tiempo que el enemigo permanece inactivo en un punto.")]
    public float idleTime = 2f;
    #endregion
}
