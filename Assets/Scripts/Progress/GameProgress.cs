using System;

/// <summary>
/// Contenedor de datos de progreso del jugador.
/// Serializable para guardado en JSON.
/// </summary>
[Serializable]
public class GameProgress
{
    /// <summary>Tiempo total jugado en segundos.</summary>
    public float tiempoJugadoSegundos;

    /// <summary>Ecopuntos acumulados del jugador.</summary>
    public int ecopuntos;

    /// <summary>Progreso total del juego (0-100).</summary>
    public float progresoTotal;

    /// <summary>
    /// Vida actual del jugador al guardar.
    /// -1 significa que no hay dato guardado (usa el máximo por defecto).
    /// </summary>
    public int vidaActual = -1;

    /// <summary>
    /// Nombre del jugador ingresado en el menú.
    /// Vacío si nunca se ha asignado un nombre.
    /// </summary>
    public string playerName = string.Empty;
}