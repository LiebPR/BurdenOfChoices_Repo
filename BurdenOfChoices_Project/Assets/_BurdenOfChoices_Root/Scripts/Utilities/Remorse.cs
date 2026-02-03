using UnityEngine;

public class Remorse : MonoBehaviour
{
    [SerializeField] EnemyFSM[] enemies;
    [SerializeField] int remorsePointsEnemy = 1;
    [SerializeField] float maxShaderValue = 5f;

    int currentRemorse;

    //Getter
    public int CurrentRemorse => currentRemorse;
    public float ShaderRemorseValue
    {
        get
        {
            if (enemies == null || enemies.Length == 0) return 0f;

            //Proporción de remordimiento acumulado respecto al total posible
            float proportion = (float)currentRemorse / (enemies.Length * remorsePointsEnemy);
            return proportion * maxShaderValue;
        }
    }

    private void Update()
    {
        CalculatRemorse();
    }

    void CalculatRemorse()
    {
        //Contador de enemigos muertos
        int deadEnemies = 0;

        //Calculo de enemigos muertos en la partida. 
        for(int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i].CurrentState == EnemyState.Death)
            {
                deadEnemies++;
            }
        }

        //Se multiplican los enemigos muertos por el remordimiento y se almacena en currentRemorse
        currentRemorse = deadEnemies * remorsePointsEnemy;

        Debug.Log($"DeadEnemies: {deadEnemies}, CurrentRemorse: {currentRemorse}");
    }
}
