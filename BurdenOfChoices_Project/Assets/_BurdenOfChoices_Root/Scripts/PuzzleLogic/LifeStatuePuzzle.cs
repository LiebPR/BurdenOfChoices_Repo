using UnityEngine;

public class LifeStatuePuzzle : MonoBehaviour, ILifeFeedback
{
    #region Inspector States
    [Header("Impulse Settings")]
    [SerializeField] Vector3 impulseDirection = Vector3.forward;
    [SerializeField] float impulseForce = 3f;
    #endregion

    #region Internal State
    bool consumed;
    #endregion

    #region References
    Rigidbody rb;
    #endregion

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    public void Consume()
    {
        if (consumed) return;

        consumed = true;
        rb.isKinematic = false;
        rb.AddForce(transform.TransformDirection(impulseDirection.normalized) * impulseForce, ForceMode.Impulse);
    }
}
