using UnityEngine;

public class PlayerRemorseFeedback : MonoBehaviour
{
    [SerializeField] Remorse remorse;
    [SerializeField] Renderer targetRenderer;

    MaterialPropertyBlock mpb;

    static readonly int BloodAmountID = Shader.PropertyToID("_BloodAmount");
    static readonly int OtherAmountID = Shader.PropertyToID("_OtherAmount");
  
    void Awake()
    {
        mpb = new MaterialPropertyBlock();
    }

    void Update()
    {
        float value = remorse.ShaderRemorseValue;
        targetRenderer.GetPropertyBlock(mpb);
        mpb.SetFloat(BloodAmountID, value);
        mpb.SetFloat(OtherAmountID, value);
        targetRenderer.SetPropertyBlock(mpb);
    }
}
