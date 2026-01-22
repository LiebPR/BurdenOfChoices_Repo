using UnityEngine;
using UnityEngine.AI;

public class EnemyMotionContext : MonoBehaviour
{
    #region Getters
    public EnemyMovementCommands Commands { get; private set; }
    public NavMeshAgent Agent { get; private set; }
    public VisionSystem Vision { get; private set; }
    #endregion

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        Vision = GetComponent<VisionSystem>();
        Commands = new EnemyMovementCommands(Agent, transform);
    }
}
