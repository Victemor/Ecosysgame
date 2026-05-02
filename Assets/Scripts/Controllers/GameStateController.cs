using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controlador central de estados del juego.
/// Es el único responsable de validar y ejecutar transiciones de estado.
/// Evita cambios directos desde otros sistemas, garantizando consistencia.
/// </summary>
public class GameStateController : MonoBehaviour
{
    private static GameStateController instance;

    /// <summary>
    /// Acceso global seguro al controlador de estados.
    /// </summary>
    public static GameStateController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameStateController>();

                if (instance == null)
                {
                    GameObject obj = new GameObject("GameStateController");
                    instance = obj.AddComponent<GameStateController>();
                }
            }
            return instance;
        }
    }

    /// <summary>
    /// Define las transiciones válidas entre estados.
    /// </summary>
    private Dictionary<GameState, List<GameState>> validTransitions;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeTransitions();
    }

    /// <summary>
    /// Configura las reglas de transición entre estados.
    /// </summary>
    private void InitializeTransitions()
    {
        validTransitions = new Dictionary<GameState, List<GameState>>
        {
            // Agrega Gameplay como destino válido desde Boot
            { GameState.Boot, new List<GameState> { GameState.MainMenu, GameState.Gameplay } },

            { GameState.MainMenu, new List<GameState>
                { GameState.Gameplay, GameState.Transition }
            },

            { GameState.Gameplay, new List<GameState>
                { GameState.Dialogue, GameState.Paused, GameState.Transition }
            },

            { GameState.Dialogue, new List<GameState>
                { GameState.Gameplay }
            },

            { GameState.Paused, new List<GameState>
                { GameState.Gameplay, GameState.MainMenu }
            },

            { GameState.Transition, new List<GameState>
                { GameState.Gameplay, GameState.MainMenu }
            }
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
            Debug.LogWarning($"Invalid transition: {currentState} → {newState}");
            return;
        }

        GameManager.Instance.SetState(newState);
    }

    /// <summary>
    /// Verifica si una transición es válida según las reglas definidas.
    /// </summary>
    private bool IsValidTransition(GameState from, GameState to)
    {
        if (!validTransitions.ContainsKey(from))
            return false;

        return validTransitions[from].Contains(to);
    }
}