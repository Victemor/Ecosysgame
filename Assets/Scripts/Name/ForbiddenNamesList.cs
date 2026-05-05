using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lista de nombres prohibidos en el juego.
/// Creada y conectada al validador, pero la comprobación está
/// desactivada hasta que el contenido de la lista sea definitivo.
/// Para activarla: cambiar 'IsActive' a true en PlayerNameValidator.
/// </summary>
[CreateAssetMenu(menuName = "Game/Player/Forbidden Names List")]
public class ForbiddenNamesList : ScriptableObject
{
    [SerializeField, Tooltip("Lista de nombres que no pueden ser usados por el jugador. " +
                             "La comparación ignora mayúsculas/minúsculas. " +
                             "INACTIVA hasta configuración final del juego.")]
    private List<string> forbiddenNames = new List<string>();

    /// <summary>
    /// Retorna true si el nombre está en la lista prohibida.
    /// La comparación es case-insensitive.
    /// </summary>
    public bool Contains(string name)
    {
        foreach (string forbidden in forbiddenNames)
        {
            if (string.Equals(name, forbidden, System.StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}