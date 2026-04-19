using System;
using UnityEngine;

/// <summary>
/// Sistema central que controla el flujo global del juego.
/// Maneja los estados principales y notifica cambios a otros sistemas desacoplados.
/// </summary>
public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    /// <summary>
    /// Acceso global seguro al GameManager.
    /// </summary>
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();

                if (instance == null)
                {
                    GameObject obj = new GameObject("GameManager");
                    instance = obj.AddComponent<GameManager>();
                }
            }
            return instance;
        }
    }

    /// <summary>
    /// Evento que se dispara cuando el estado del juego cambia.
    /// </summary>
    public event Action<GameState> OnGameStateChanged;

    /// <summary>
    /// Estado actual del juego.
    /// </summary>
    public GameState CurrentState { get; private set; }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SetState(GameState.Boot);
    }

    /// <summary>
    /// Cambia el estado global del juego.
    /// Solo debe ser llamado por GameStateController.
    /// </summary>
    internal void SetState(GameState newState)
    {
        if (CurrentState == newState)
            return;

        CurrentState = newState;
        OnGameStateChanged?.Invoke(CurrentState);
    }
}