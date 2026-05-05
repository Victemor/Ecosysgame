using UnityEngine;

/// <summary>
/// Contenedor de sistemas globales persistentes entre escenas.
/// Aloja SceneLoader, CurrencyManager y otros sistemas globales.
/// Se auto-destruye si ya existe una instancia.
/// </summary>
public class GamePersistence : MonoBehaviour
{
    private static GamePersistence instance;

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
}