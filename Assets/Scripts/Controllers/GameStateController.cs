using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controlador central de estados del juego.
/// Es el único responsable de validar y ejecutar transiciones de estado.
/// </summary>
public class GameStateController : MonoBehaviour
{
    private static GameStateController instance;

    public static GameStateController Instance
    {
        get
        {
            if (instance == null)
                instance = FindFirstObjectByType<GameStateController>();
            return instance;
        }
    }

    private Dictionary<GameState, List<GameState>> validTransitions;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeTransitions();
    }

    private void InitializeTransitions()
    {
        validTransitions = new Dictionary<GameState, List<GameState>>
        {
            { GameState.Boot,      new List<GameState> { GameState.MainMenu, GameState.Gameplay } },
            { GameState.MainMenu,  new List<GameState> { GameState.Gameplay, GameState.Transition } },
            { GameState.Gameplay,  new List<GameState> { GameState.Dialogue, GameState.Paused, GameState.Transition } },
            { GameState.Dialogue,  new List<GameState> { GameState.Gameplay } },
            { GameState.Paused,    new List<GameState> { GameState.Gameplay, GameState.MainMenu } },
            { GameState.Transition,new List<GameState> { GameState.Gameplay, GameState.MainMenu } }
        };
    }

    /// <summary>
    /// Solicita un cambio de estado.
    /// Valida si la transición es permitida antes de aplicarla.
    /// </summary>
    public void RequestState(GameState newState)
    {
        GameState currentState = GameManager.Instance.CurrentState;

        if (!IsValidTransition(currentState, newState))
        {
            Debug.LogWarning($"[GameStateController] Transición inválida: {currentState} → {newState}");
            return;
        }

        GameManager.Instance.SetState(newState);
    }

    private bool IsValidTransition(GameState from, GameState to)
    {
        if (!validTransitions.ContainsKey(from)) return false;
        return validTransitions[from].Contains(to);
    }
}