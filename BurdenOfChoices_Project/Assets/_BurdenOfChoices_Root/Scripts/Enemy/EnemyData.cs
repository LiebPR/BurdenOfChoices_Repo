using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "EnemyData/Enemy")]
public class EnemyData : ScriptableObject
{

    //Perception
    [Header("Vision Settings")]
    public float visionRadius = 6f; //radio de visión del enemigo
    public float visionAngle = 45f; //ángulo de visión del enemigo
    public float perceptionRadius = 1f;
    public LayerMask obstacleMask;

    [Header("Vision Delays")]
    public float perceptionDelay = 0.5f; //tiempo de retraso para detectar el área
    public float lostDelay = 1f; //tiempo de retraso para perder al jugador

    [Header("Stop Area")]
    public float stopAreaRadius = 2f; //radio del área donde el enemigo se detiene
    public float minStopDistance = 1.5f; //distancia minima de parada
    public float maxStopDistance = 3.5f; //distancia máxima de parada
    public float stopTransitionTime = 0.5f; //tiempo de transición al frenar

    //States 
    [Header("Movement Settings")]
    public float patrolSpeed = 3f; //velocidad de patrulla del enemigo
    public float chaseSpeed = 5f; //velocidad de persecución del enemigo
    public float rotationSpeed = 8f; //velocidad de rotación del enemigo
    public float destinationUpdateThreshold = 0.2f;

    [Header("Rotation Damp Settings")]
    public float rotationStiffness = 6f; //fuerza del resorte
    public float rotationDamping = 10f; //fricción angular

    [Header("Acceleration Settings")]
    public float normalAcceleration = 8f;
    public float breackAcceleration = 25f;

    [Header("Idle Settings")]
    public float idleTime = 2f; //tiempo que el enemigo permanece inactivo en un punto de patrulla
}
