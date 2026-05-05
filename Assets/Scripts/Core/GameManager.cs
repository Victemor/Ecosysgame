using System;
using UnityEngine;

/// <summary>
/// Sistema central que controla el flujo global del juego.
/// Solo establece Boot en Start — cada escena pide su propio estado.
/// </summary>
public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindFirstObjectByType<GameManager>();
            return instance;
        }
    }

    public event Action<GameState> OnGameStateChanged;
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
        // Solo establece Boot. Cada escena pide su propio estado a través
        // de MainMenuController o GameplaySceneBootstrap.
        SetState(GameState.Boot);
    }

    internal void SetState(GameState newState)
    {
        if (CurrentState == newState) return;

        CurrentState = newState;
        OnGameStateChanged?.Invoke(CurrentState);
    }
}