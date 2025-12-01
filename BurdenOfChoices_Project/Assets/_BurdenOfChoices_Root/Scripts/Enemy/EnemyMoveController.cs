using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMoveController : MonoBehaviour
{
    public EnemyMovementCommands Commands { get; private set; }
    public NavMeshAgent Agent { get; private set; }
    public VisionSystem Vision { get; private set; }

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        Vision = GetComponent<VisionSystem>();
        Commands = new EnemyMovementCommands(Agent, transform);
    }
}
