using Unity.VisualScripting;
using UnityEngine;

public class EnemyFootsStepsUpdater : MonoBehaviour
{
    EnemyFSM fsm;
    EnemyMotionContext motionContext;
    AudioEmitter emitter;

    private void Awake()
    {
        fsm = GetComponent<EnemyFSM>();
        motionContext = GetComponent<EnemyMotionContext>();
    }

    private void Update()
    {
        UpdateFootSteps();
    }

    void UpdateFootSteps()
    {
        float speed = motionContext.Agent.velocity.magnitude;

        if (speed <= 0.05f)
        {
            if(emitter != null)
            {
                AudioManager.Instance.StopSFX(emitter);
                emitter = null;
            }
            return;
        }

        //En caso de que no exista el emitter se crea
        if (emitter == null)
        {
            emitter = AudioManager.Instance.PlaySFXAttached("SFX_Sectario_Walk", transform);
            Debug.Log("Buen Audio Bro");
        }

        //Ajuste de Pitch según el estado
        AudioSource src = emitter.GetComponent<AudioSource>();

        if (fsm.CurrentState == EnemyState.Chase)
            src.pitch = 1.5f;
        else
            src.pitch = 1.2f;
    }
}
