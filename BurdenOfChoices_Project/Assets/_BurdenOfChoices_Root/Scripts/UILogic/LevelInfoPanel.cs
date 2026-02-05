using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelInfoPanel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] TextMeshProUGUI levelNameText;
    [SerializeField] SceneButton playButton;

    [Header("PeackUps UI")]
    [SerializeField] Image[] peackUpIcons;
    [SerializeField] Color activeColor = Color.white;
    [SerializeField] Color inactiveColor = Color.black;

    private MeshButtonSelectable currentPlant;

    // Se llama desde FlowManager al seleccionar un MeshButton
    public void SetLevel(MeshButtonSelectable plant)
    {
        currentPlant = plant;

        // Actualizamos el texto del nivel
        levelNameText.text = plant.LevelData.levelName;
        // Configuramos SceneButton con la escena del nivel
        playButton.SetScene(plant.LevelData.sceneName);

        UpdatePeackUpUI(plant.LevelData);
    }

    // Se llama desde el botón Play
    public void OnPlayButtonPressed()
    {
        // Activamos la cámara del nivel usando CameraManager
        if (currentPlant != null && currentPlant.PlayCamera != null && CameraManager.Instance != null)
        {
            // Cambiamos la prioridad de la cámara del nivel
            CameraManager.Instance.ActivateCamera(currentPlant.PlayCamera);
        }

        // Ocultamos el panel
        gameObject.SetActive(false);

        // Llamamos al SceneButton para cargar la escena
        playButton.LoadSceneDelay();
    }

    #region UI Logic
    void UpdatePeackUpUI(LevelData data)
    {
        for(int i = 0; i < peackUpIcons.Length; i++)
        {
            peackUpIcons[i].color =
                i < data.collectedPeackUps ? activeColor : inactiveColor;
        }
    }
    #endregion
}
