using UnityEngine;

/// <summary>
/// DataProvider: Componente que expone cualquier ScriptableObject desde el inspector.
/// Diseñado para cualquier objeto que contenga o necesite un SO.
/// </summary>
public class DataProvider : MonoBehaviour
{
    [Header("SO")]
    [SerializeField] ScriptableObject data;

    public ScriptableObject Data => data;

    //Obtiene el ScriptableObject tipado o null si no coincide.
    public T GetData<T>() where T : ScriptableObject
    {
        return data as T;
    }

    //Intenta obtener el ScriptableObject tipado.
    public void SetData(ScriptableObject newData)
    {
        data = newData;
    }

    private void Awake()
    {
        if (data == null)
        {
            Debug.LogWarning($"{nameof(DataProvider)} en '{gameObject.name}' no tiene asignado ningún ScriptableObject.");
        }
    }
}
