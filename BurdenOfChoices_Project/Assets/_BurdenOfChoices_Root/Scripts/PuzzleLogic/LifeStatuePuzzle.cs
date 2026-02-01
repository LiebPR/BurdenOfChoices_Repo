using UnityEngine;

public class LifeStatuePuzzle : MonoBehaviour, ILifeFeedback
{
    #region Inspector States
    [Header("Explosion Statue VFX")]
    [SerializeField] GameObject explosionVFX;
    #endregion

    #region Internal State
    bool consumed;
    #endregion

    public void Consume()
    {
        if (consumed) return;

        consumed = true;
        Instantiate(explosionVFX, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
