using System.Collections;
using UnityEngine;

public class Pillar : MonoBehaviour
{
    #region Inspector States
    [SerializeField] int requiredObjectID;
    [SerializeField] Statue statue;

    [Header("Feedback")]
    [SerializeField] float pushDistance = 0.15f;
    [SerializeField] float pushDuration = 0.2f;

    [Header("Puzzle Reference")]
    [SerializeField] StatuePuzzleController puzzleController;
    #endregion

    #region Internal States
    bool activated;
    Vector3 originalPosition;
    #endregion

    private void Awake()
    {
        originalPosition = transform.position;
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (activated) return;

        PuzzleThrowItem throwable = collision.collider.GetComponent<PuzzleThrowItem>();
        if (throwable == null) return;

        if (throwable.GetObjectID() != requiredObjectID)
        {
            if(puzzleController != null)
            {
                puzzleController.OnWrongHit();
            }
            return;
        }

        activated = true;
        StartCoroutine(PushReaction());
        statue.DropStatue(transform.forward);
    }

    #region Feedback
    IEnumerator PushReaction()
    {
        Vector3 backPos = transform.position - transform.forward * pushDistance;

        float t = 0f;
        while(t < pushDuration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(originalPosition, backPos, t / pushDuration);
            yield return null;
        }

        t = 0f;
        while(t < pushDuration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(originalPosition, backPos, t / pushDuration);
            yield return null;
        }

        t = 0f;
        while(t < pushDuration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(backPos, originalPosition, t / pushDuration);
            yield return null;
        }
    }
    #endregion
}
