using UnityEngine;

public class MenuBootstrap : MonoBehaviour
{
    [Header("Canvases")]
    [SerializeField] GameObject startCanvas;

    [Header("Flow")]
    [SerializeField] FlowManager flowManager;

    void Awake()
    {
        if (GameFlowContext.ReturnFromLevel)
        {
            //Saltamos el Start Canvas
            startCanvas.SetActive(false);
        }
        else
        {
            //Arranque normal
            startCanvas.SetActive(true);
        }
    }
}
