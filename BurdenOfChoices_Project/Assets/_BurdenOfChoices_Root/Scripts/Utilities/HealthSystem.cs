using System;
using UnityEngine;

/// <summary>
/// Sistema genérico de vida.
/// No decide comportamiento, solo emite eventos.
/// </summary>
public class HealthSystem : MonoBehaviour
{
    #region Inspector Variables
    [Header("Health Config")]
    [SerializeField] int maxHealth = 3; //vida máxima en golpes
    #endregion

    #region Internal State
    int currentHealth; //vida actual
    #endregion

    #region Events
    public event Action<int> OnHit; //golpe recibido
    public event Action<int> OnHealthChanged; //cambio de vida
    public event Action OnDeath; //vida llegada a 0
    #endregion

    #region Getters
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsAlive => currentHealth > 0;
    #endregion

    private void Awake()
    {
        //Inicialización base
        currentHealth = maxHealth;
    }

    #region Health Logic 
    /// <summary>
    /// Aplica daño directo.
    /// </summary>
    public void TakeHit(int damage = 1)
    {
        if (!IsAlive) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        OnHit?.Invoke(damage);
        if(currentHealth <= 0)
        {
            OnDeath?.Invoke();
        }
    }

    /// <summary>
    /// Restaura la vida al máximo.
    /// </summary>
    public void HealFull()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth);
    }
    #endregion
}
