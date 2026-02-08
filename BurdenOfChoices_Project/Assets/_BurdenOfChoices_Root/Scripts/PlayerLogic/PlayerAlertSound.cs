using UnityEngine;

public class PlayerAlertSound : MonoBehaviour
{

    private void OnEnable()
    {
        EnemyFSM[] enemies = FindObjectsByType<EnemyFSM>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
            enemy.OnStateChanged += OnEnemyStateChanged;
    }

    private void OnDisable()
    {
        EnemyFSM[] enemies = FindObjectsByType<EnemyFSM>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
            enemy.OnStateChanged -= OnEnemyStateChanged;
    }

    void OnEnemyStateChanged(EnemyState newState)
    {

        if (newState == EnemyState.InvestigateSound)
        {
            AudioManager.Instance.PlaySFX2D("SFX_Grace_Alert", 0.7f);
        }
    }
}
