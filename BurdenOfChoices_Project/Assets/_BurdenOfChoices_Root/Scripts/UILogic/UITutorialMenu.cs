using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class UITutorialMenu : MonoBehaviour
{
    #region Inspector
    [SerializeField] TextMeshProUGUI tutorialText;
    [SerializeField] float blinkSpeed = 2f;
    #endregion

    Coroutine blinkRoutine;
    Action onHiddenCallback;

    /// <summary>
    /// Muestra el texto de tutorial con animación.
    /// </summary>
    public void Show(string text, Action onHidden)
    {
        onHiddenCallback = onHidden;

        tutorialText.text = text;
        tutorialText.gameObject.SetActive(true);

        StopBlink();
        blinkRoutine = StartCoroutine(BlinkRoutine());
    }

    /// <summary>
    /// Oculta el texto y notifica a la misión que lo solicitó.
    /// </summary>
    public void Hide()
    {
        StopBlink();

        if (blinkRoutine != null)
            StopCoroutine(blinkRoutine);

        StartCoroutine(HideRoutine());
    }

    #region Animation
    IEnumerator BlinkRoutine()
    {
        tutorialText.ForceMeshUpdate();
        TMP_TextInfo textInfo = tutorialText.textInfo;

        float time = 0f;

        while (true)
        {
            tutorialText.ForceMeshUpdate();
            textInfo = tutorialText.textInfo;

            int charCount = textInfo.characterCount;

            for (int i = 0; i < charCount; i++)
            {
                if (!textInfo.characterInfo[i].isVisible)
                    continue;

                int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
                int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;
                Color32[] colors = textInfo.meshInfo[materialIndex].colors32;

                // OLA VERTICAL
                float wave = Mathf.Sin(time * blinkSpeed + i * 0.3f);
                Vector3 offset = Vector3.up * wave * 5f;

                vertices[vertexIndex + 0] += offset;
                vertices[vertexIndex + 1] += offset;
                vertices[vertexIndex + 2] += offset;
                vertices[vertexIndex + 3] += offset;

                // OPACIDAD POR CARÁCTER
                byte alpha = (byte)Mathf.Lerp(80, 255, (wave + 1f) * 0.8f);

                colors[vertexIndex + 0].a = alpha;
                colors[vertexIndex + 1].a = alpha;
                colors[vertexIndex + 2].a = alpha;
                colors[vertexIndex + 3].a = alpha;
            }

            // Aplicar cambios
            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                textInfo.meshInfo[i].mesh.colors32 = textInfo.meshInfo[i].colors32;
                tutorialText.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }

            time += Time.deltaTime;
            yield return null;
        }
    }
    IEnumerator HideRoutine()
    {
        tutorialText.ForceMeshUpdate();
        TMP_TextInfo textInfo = tutorialText.textInfo;

        float duration = 0.5f; // duración de la animación
        float time = 0f;

        // Guardamos las posiciones originales
        Vector3[][] originalVertices = new Vector3[textInfo.meshInfo.Length][];
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
            originalVertices[i] = (Vector3[])textInfo.meshInfo[i].vertices.Clone();

        while (time < duration)
        {
            tutorialText.ForceMeshUpdate();
            textInfo = tutorialText.textInfo;
            float t = time / duration;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                if (!textInfo.characterInfo[i].isVisible)
                    continue;

                int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
                int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;
                Color32[] colors = textInfo.meshInfo[materialIndex].colors32;

                // Desplazar hacia abajo
                Vector3 offset = Vector3.down * 20f * t;

                vertices[vertexIndex + 0] = originalVertices[materialIndex][vertexIndex + 0] + offset;
                vertices[vertexIndex + 1] = originalVertices[materialIndex][vertexIndex + 1] + offset;
                vertices[vertexIndex + 2] = originalVertices[materialIndex][vertexIndex + 2] + offset;
                vertices[vertexIndex + 3] = originalVertices[materialIndex][vertexIndex + 3] + offset;

                // Fade a transparente
                byte alpha = (byte)Mathf.Lerp(255, 0, t);
                colors[vertexIndex + 0].a = alpha;
                colors[vertexIndex + 1].a = alpha;
                colors[vertexIndex + 2].a = alpha;
                colors[vertexIndex + 3].a = alpha;
            }

            // Aplicar cambios
            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                textInfo.meshInfo[i].mesh.colors32 = textInfo.meshInfo[i].colors32;
                tutorialText.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }

            time += Time.deltaTime;
            yield return null;
        }

        // Finalmente apagar
        tutorialText.gameObject.SetActive(false);
        onHiddenCallback?.Invoke();
        onHiddenCallback = null;
    }

    void StopBlink()
    {
        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
        }
    }
    #endregion
}
