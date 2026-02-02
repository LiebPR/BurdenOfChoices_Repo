using UnityEngine;

public class Remorse : MonoBehaviour
{
    [SerializeField] EnemyFSM[] enemies;
    [SerializeField] int remorsePointsEnemy = 1;
    float maxShaderValue;

    int currentRemorse;

    //Getter
    public int CurrentRemorse => currentRemorse;
    public float ShaderRemorseValue
    {
        get
        {
            if (enemies.Length == 0) return 0f;
            return((float)currentRemorse / enemies.Length) * maxShaderValue;
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
    }
}
