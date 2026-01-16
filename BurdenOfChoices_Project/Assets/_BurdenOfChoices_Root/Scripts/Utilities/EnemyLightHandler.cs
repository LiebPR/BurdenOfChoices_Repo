using UnityEngine;

public class EnemyLightHandler : MonoBehaviour
{
    [SerializeField] Light enemyLight;
    [SerializeField] Light enemyLight2;

    public void TurnOn()
    {
        if (enemyLight != null)
            enemyLight.enabled = true;
        if(enemyLight2 != null)
            enemyLight2.enabled = true;
    }

    public void TurnOff()
    {
        if (enemyLight != null)
            enemyLight.enabled = false;
        if(enemyLight2 != null)
            enemyLight2.enabled = false;
    }
}
