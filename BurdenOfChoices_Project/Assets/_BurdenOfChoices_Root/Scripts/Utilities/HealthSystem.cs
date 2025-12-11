using System;
using UnityEngine;

/// <summary>
/// HealthSystem: Componente general para gestionar vida y golpes.
/// </summary>
public class HealthSystem : MonoBehaviour
{
    #region Inspector Variables
    [Header("Health Config")]
    [SerializeField] int maxHealth = 3; //vida máxima en golpes
    #endregion

    #region Internal State
    int currentHealth;
    #endregion

    #region Events
    public event Action<int> OnHit; //se disapra cuando el objeto recibe un golpe
    public event Action<int> OnHealthChanged; //se dispara cuando la vida cambia
    public event Action OnDeath; //se dispara cuando llega a 0
    #endregion

    #region Getters
    public int CurrentHealth => currentHealth;
    public int MaxHEalth => maxHealth;
    public bool IsAlive => currentHealth > 0;
    #endregion

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    #region Health Logic 
    public void TakeHit(int damage = 1)
    {
        if (!IsAlive) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        if(currentHealth <= 0)
        {
            OnDeath?.Invoke();
        }
    }

    //Restauramos la vida a la máxima
    public void HealFull()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth);
    }
    #endregion
}
