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
            // Destroy(this) destruye solo el componente, NO el GameObject.
            // Destroy(gameObject) destruiría todo el GamePersistence,
            // matando ProgressManager, SceneLoader, etc. en el mismo objeto.
            Destroy(this);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SetState(GameState.Boot);
    }

    internal void SetState(GameState newState)
    {
        if (CurrentState == newState) return;

        CurrentState = newState;
        OnGameStateChanged?.Invoke(CurrentState);
    }
}