using UnityEngine;

/// <summary>
/// Contenedor de sistemas globales persistentes entre escenas.
/// Aloja SceneLoader, CurrencyManager y otros sistemas globales.
/// Se auto-destruye solo a sí mismo (no al GameObject) si ya existe una instancia,
/// para no matar los demás componentes que conviven en este objeto.
/// </summary>
public class GamePersistence : MonoBehaviour
{
    private static GamePersistence instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}