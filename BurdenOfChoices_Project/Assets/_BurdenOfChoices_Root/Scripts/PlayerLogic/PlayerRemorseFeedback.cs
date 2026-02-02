using UnityEngine;

public class PlayerRemorseFeedback : MonoBehaviour
{
    [SerializeField] Remorse remorse;
    Material material;

    private void Awake()
    {
        material = GetComponent<Material>();
    }

    private void Update()
    {
        float i = remorse.ShaderRemorseValue;

        material.SetFloat("_BloodAmount", i);
        material.SetFloat("_OtherAmount", i);
    }
}
