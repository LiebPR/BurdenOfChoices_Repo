using System.Collections;
using UnityEngine;

/// <summary>
/// Script que asegura un FadeIn al entrar en la escena.
/// Colócalo en un GameObject de la escena para que siempre se haga de negro a transparente.
/// </summary>
public class SceneFadeIn : MonoBehaviour
{
    [SerializeField] float fadeDelay;
    private void Start()
    {
        StartCoroutine(FadeDelay());
    }

    IEnumerator FadeDelay()
    {
        yield return new WaitForSeconds(fadeDelay);
        if (FadeController.Instance != null)
            StartCoroutine(FadeController.Instance.FadeIn());
    }
}
