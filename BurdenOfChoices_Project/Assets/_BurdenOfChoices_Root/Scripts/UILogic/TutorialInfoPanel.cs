using TMPro;
using UnityEngine;

public class TutorialInfoPanel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] TextMeshProUGUI tutorialTitleText;  // Título del tutorial

    private string currentTutorialTitle;

    // Se llama desde FlowManager al seleccionar un tutorial
    public void SetTutorial(string tutorialTitle)
    {
        currentTutorialTitle = tutorialTitle;

        // Actualizamos el texto del título del tutorial
        tutorialTitleText.text = currentTutorialTitle;

        UpdateTutorialContent();
    }

    // Método para actualizar el contenido del tutorial (si se necesita más información)
    void UpdateTutorialContent()
    {
        gameObject.SetActive(true);  // Activa el contenido del tutorial (si es necesario)
    }

    // Método para ocultar el panel (cuando se finaliza o se cierra el tutorial)
    public void HidePanel()
    {
        gameObject.SetActive(false);  // Oculta el panel del tutorial
    }
}
