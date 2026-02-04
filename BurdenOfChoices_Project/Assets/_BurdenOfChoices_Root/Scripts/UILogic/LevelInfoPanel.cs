using TMPro;
using UnityEngine;
using Unity.Cinemachine;

public class LevelInfoPanel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] TextMeshProUGUI levelNameText;
    [SerializeField] SceneButton playButton;

    private MeshButtonSelectable currentPlant;

    // Se llama desde FlowManager al seleccionar un MeshButton
    public void SetLevel(MeshButtonSelectable plant)
    {
        currentPlant = plant;

        // Actualizamos el texto del nivel
        levelNameText.text = plant.LevelData.levelName;

        // Configuramos SceneButton con la escena del nivel
        playButton.SetScene(plant.LevelData.sceneName);
    }

    // Se llama desde el botón Play
    public void OnPlayButtonPressed()
    {
        // Activamos la cámara del nivel usando CameraManager
        if (currentPlant != null && currentPlant.LevelCamera != null && CameraManager.Instance != null)
        {
            // Cambiamos la prioridad de la cámara del nivel
            CameraManager.Instance.ActivateCamera(currentPlant.LevelCamera);
        }

        // Ocultamos el panel
        gameObject.SetActive(false);

        // Llamamos al SceneButton para cargar la escena
        playButton.LoadSceneDelay();
    }
}
