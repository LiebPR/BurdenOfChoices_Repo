using UnityEngine;
using System.Collections;

public class MaterialButtonVisual : MonoBehaviour
{
    [Header("Materials")]
    [Tooltip("MeshRenderer que cambiará de material según el estado del botón")]
    [SerializeField] MeshRenderer targetMesh;

    [Tooltip("Material por defecto cuando el botón está en estado normal (sin hover ni selección)")]
    [SerializeField] Material normal;

    [Tooltip("Material que se usa cuando el cursor está encima del botón")]
    [SerializeField] Material hover;

    [Tooltip("Material que se aplica cuando el botón ha sido seleccionado")]
    [SerializeField] Material selected;

    [Tooltip("Material usado cuando el botón está deshabilitado o bloqueado por el flujo")]
    [SerializeField] Material disabled;


    [Header("Hover Settings")]
    [Tooltip("Duración de efecto de hover de entrada")]
    [SerializeField] bool hoverFlashing = false;
    [SerializeField] float hoverFlashDuration = 0.15f;

    Material current;
    Coroutine hoverRoutine;

    private void Awake()
    {
        if (targetMesh == null)
        {
            Debug.LogError("Asiganar targetMesh en" + name);
            enabled = false;
            return;
        }

        if (normal == null)
            normal = targetMesh.material;

        Set(normal);
    }

    public void SetNormal() => Set(normal);
    public void SetSelected() => Set(selected ?? normal);
    public void SetDisabled() => Set(disabled ?? normal);


    public void SetHover()
    {
        if (hover == null)
            return;

        if (!hoverFlashing)
        {
            Set(hover);
        }
    }

    public void OnHoverEnter()
    {
        if (hover == null)
            return;

        if (hoverFlashing)
        {
            if (hoverRoutine != null)
                StopCoroutine(hoverRoutine);

            hoverRoutine = StartCoroutine(HoverFlash());
        }
        else
        {
            Set(hover);
        }
    }

    IEnumerator HoverFlash()
    {
        // Flash rápido
        Set(hover);
        yield return new WaitForSeconds(hoverFlashDuration);

        // Hover normal después del flash
        Set(hover);

        hoverRoutine = null;
    }

    void Set(Material mat)
    {
        if (current == mat) return;
        current = mat;
        targetMesh.material = mat;
    }
}
