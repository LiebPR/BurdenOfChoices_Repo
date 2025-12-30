using UnityEngine;
using UnityEngine.UI;

public class EnemyPerceptionFeedback : MonoBehaviour
{
    #region Inspector States
    [Header("UI Images")]
    [SerializeField] Image suspiciousImage; // ?
    [SerializeField] Image alertImage; // !
    [SerializeField] Image stunImage; // Stun
    #endregion

    #region References
    EnemyFSM fsm;
    VisionSystem vision;
    StunState stun;
    #endregion

    private void Awake()
    {
        fsm = GetComponent<EnemyFSM>();
        vision = GetComponent<VisionSystem>();
        stun = GetComponent<StunState>();
    }

    private void OnEnable()
    {
        if (vision != null)
        {
            vision.OnEnterPerception += HandlePerception;
            vision.OnSeeTarget += HandleSeeTarget;
            vision.OnLoseTarget += HandleLoseTarget;
        }
    }

    private void OnDisable()
    {
        if (vision != null)
        {
            vision.OnEnterPerception -= HandlePerception;
            vision.OnSeeTarget -= HandleSeeTarget;
            vision.OnLoseTarget -= HandleLoseTarget;
        }
    }

    private void Update()
    {
        UpdateFeedback();
    }

    void HandlePerception(Transform target)
    {
        // Solo mostramos sospecha si no tiene visión directa ni está stuneado
        if (stun != null && stun.IsStunned) return;
        if (vision.CanSeeTarget()) return;

        // Mostrar imagen de sospecha durante el delay
        suspiciousImage.enabled = true;
    }

    void HandleSeeTarget(Transform target)
    {
        suspiciousImage.enabled = false;
        alertImage.enabled = true;
    }

    void HandleLoseTarget(Transform target)
    {
        suspiciousImage.enabled = false;
        alertImage.enabled = false;
    }

    void UpdateFeedback()
    {
        // Prioridad absoluta: Stun
        if (stun != null && stun.IsStunned)
        {
            DisableAll();
            stunImage.enabled = true;
            return;
        }

        // Visión directa
        if (vision != null && vision.CanSeeTarget())
        {
            DisableAll();
            alertImage.enabled = true;
            return;
        }

        // Estado de sospecha por percepción
        if (fsm.CurrentState == EnemyState.Alert || fsm.CurrentState == EnemyState.TurnToTarget)
        {
            DisableAll();
            suspiciousImage.enabled = true;
            return;
        }

        // Default: todo apagado
        DisableAll();
    }

    void DisableAll()
    {
        suspiciousImage.enabled = false;
        alertImage.enabled = false;
        stunImage.enabled = false;
    }
}