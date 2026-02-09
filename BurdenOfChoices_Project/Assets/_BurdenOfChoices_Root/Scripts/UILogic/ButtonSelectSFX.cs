using UnityEngine;

public class ButtonSelectSFX : MonoBehaviour
{
    [SerializeField] string selectButtonSFX = "SFX_UI_";
    [SerializeField] float volumen = 0.8f;

    public void OnButtonSelectedSFX()
    {
        AudioManager.Instance.PlaySFX2D(selectButtonSFX, volumen);
    }
}
