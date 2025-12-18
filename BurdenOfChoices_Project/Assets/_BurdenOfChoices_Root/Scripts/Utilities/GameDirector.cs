using System;
using UnityEngine;

public class GameDirector : MonoBehaviour
{
    //Singelton accesible globalmente
    public static GameDirector Instance;

    public GamePhase currentPhase = GamePhase.Menu; //fase actual del juego
    public GameOutcome currentOutcome = GameOutcome.None;

    #region Events
    //Eventos que otros sistemas pueden suscribirse
    public event Action<GamePhase> OnPhaseChanged;
    public event Action<GameOutcome> OnOutcomeChanged;
    #endregion

    private void Awake()
    {
        //Singelton simple
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        //Asegura coherencia inicial
        NotifyPhaseChanged(currentPhase);
        NotifyOutcomeChanged(currentOutcome);
    }

    #region API
    //Cambia la fase del juego y notifica a los oyentes
    public void SetPhase(GamePhase newPhase)
    {
        if(currentPhase == newPhase) return;
        currentPhase = newPhase;
        NotifyPhaseChanged(newPhase);
    }

    public void SetOutcome(GameOutcome outcome)
    {
       if(currentOutcome == outcome) return;
       currentOutcome = outcome;
        NotifyOutcomeChanged(outcome);
    }

    //Resetear el outcome a None
    public void ResetOutcome()
    {
        SetOutcome(GameOutcome.None);
    }
    #endregion

    #region Helpers
    void NotifyPhaseChanged(GamePhase phase)
    {
        OnPhaseChanged?.Invoke(phase);
    }
    void NotifyOutcomeChanged(GameOutcome outcome)
    {
        OnOutcomeChanged?.Invoke(outcome);
    }
    #endregion
}

#region Enums
public enum GamePhase
{
    Menu, 
    Playing,
    Cutscene,
    Paused
}

public enum GameOutcome
{
    None,
    GoodEnding,
    BadEnding,
    RespawnLose,
    NormalLose
}
#endregion
