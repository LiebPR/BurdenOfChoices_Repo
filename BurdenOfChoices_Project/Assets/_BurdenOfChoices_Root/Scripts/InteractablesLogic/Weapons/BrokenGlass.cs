using System.Collections;
using UnityEngine;

public class BrokenGlass : MonoBehaviour
{
    #region Inspector States
    [Header("Break Settings")]
    [SerializeField] LayerMask breakLayers;
    [SerializeField] int hitsToBreak = 1;
    [SerializeField] float breakDelay = 0.1f;
    [SerializeField] GameObject breakVFX;
    #endregion

    #region Internal States
    int currentHits;
    bool isBroken;
    Coroutine breakRoutine;
    Rigidbody rb;
    #endregion

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isBroken) return;

        //Impacto con capas rompibles (suelo, entorno) 
        if (IsBreakLayer(collision.collider.gameObject.layer))
        {
            return;
        }

        RegisterHit(collision.contacts[0].point);
    }

    #region Public API
    public void BreakImmediate(Vector3 position)
    {
        if (isBroken) return;

        // Cancelar cualquier rotura diferida
        if (breakRoutine != null)
        {
            StopCoroutine(breakRoutine);
            breakRoutine = null;
        }

        Break(position);
    }
    #endregion

    #region Internal Logic
    void Break(Vector3 position)
    {
        isBroken = true;

        // Forzar que se suelte si es un objeto pickable
        PickableBehaviour pickable = GetComponent<PickableBehaviour>();
        if (pickable != null && pickable.IsCatched)
        {
            pickable.OnDrop(true); // fuerza el drop y activa física
        }

        if (breakVFX != null)
        {
            Instantiate(breakVFX, position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    void RegisterHit(Vector3 hitpoint)
    {
        currentHits++;

        if (currentHits < hitsToBreak)
            return;

        if(breakRoutine == null)
            breakRoutine = StartCoroutine(BreakAfterDelay(hitpoint));
    }
    bool IsBreakLayer(int layer)
    {
        return (breakLayers.value & (1 << layer)) != 0;
    }
    #endregion

    #region Routine
    IEnumerator BreakAfterDelay(Vector3 position)
    {
        yield return new WaitForSeconds(breakDelay);
        Break(position);
    }
    #endregion
}
