using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Controlador central del sistema climático.
/// Maneja un único evento climático activo y coordina su ejecución.
/// </summary>
public class ClimateController : MonoBehaviour
{
    private static ClimateController instance;

    /// <summary>
    /// Acceso global seguro.
    /// </summary>
    public static ClimateController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ClimateController>();

                if (instance == null)
                {
                    GameObject obj = new GameObject("ClimateController");
                    instance = obj.AddComponent<ClimateController>();
                }
            }
            return instance;
        }
    }

    /// <summary>
    /// Evento cuando inicia un evento climático.
    /// </summary>
    public event Action<ClimateEventData> OnClimateEventStarted;

    /// <summary>
    /// Evento cuando finaliza un evento climático.
    /// </summary>
    public event Action<ClimateEventData> OnClimateEventEnded;

    private ClimateProfile currentProfile;
    private ClimateEventData activeEvent;

    private Coroutine climateRoutine;
    private Coroutine activeEventRoutine;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    /// <summary>
    /// Inicializa el sistema climático con un perfil de nivel.
    /// </summary>
    public void Initialize(ClimateProfile profile)
    {
        currentProfile = profile;

        if (climateRoutine != null)
            StopCoroutine(climateRoutine);

        climateRoutine = StartCoroutine(ClimateLoop());
    }

    /// <summary>
    /// Loop principal que decide cuándo activar eventos.
    /// </summary>
    private IEnumerator ClimateLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(currentProfile.TimeBetweenEvents);

            if (activeEvent != null)
                continue;

            float roll = UnityEngine.Random.value;

            if (roll <= currentProfile.EventProbability)
            {
                ClimateEventData randomEvent = GetRandomEvent();
                StartClimateEvent(randomEvent);
            }
        }
    }

    /// <summary>
    /// Inicia un evento climático manualmente (desde diálogo o interacción).
    /// </summary>
    public void StartClimateEvent(ClimateEventData eventData)
    {
        if (eventData == null)
            return;

        if (activeEvent != null)
        {
            EndCurrentEvent();
        }

        activeEvent = eventData;

        OnClimateEventStarted?.Invoke(activeEvent);

        ApplyEffects(activeEvent.Effect);

        activeEventRoutine = StartCoroutine(EventDurationRoutine(activeEvent.Duration));
    }

    /// <summary>
    /// Rutina que controla la duración del evento.
    /// </summary>
    private IEnumerator EventDurationRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        EndCurrentEvent();
    }

    /// <summary>
    /// Finaliza el evento actual.
    /// </summary>
    private void EndCurrentEvent()
    {
        if (activeEvent == null)
            return;

        ResetEffects(activeEvent.Effect);

        OnClimateEventEnded?.Invoke(activeEvent);

        activeEvent = null;
    }

    /// <summary>
    /// Aplica efectos del evento climático.
    /// </summary>
    private void ApplyEffects(ClimateEffect effect)
    {
        if (effect == null)
            return;

        // Aquí NO ejecutamos lógica directa compleja
        // Solo ejemplo base (luego se conecta a otros sistemas)

        Debug.Log($"Aplicando efectos climáticos: Agua {effect.WaterLevelModifier}");
    }

    /// <summary>
    /// Revierte efectos aplicados.
    /// </summary>
    private void ResetEffects(ClimateEffect effect)
    {
        if (effect == null)
            return;

        Debug.Log("Reseteando efectos climáticos");
    }

    /// <summary>
    /// Obtiene un evento aleatorio del perfil.
    /// </summary>
    private ClimateEventData GetRandomEvent()
    {
        var list = currentProfile.PossibleEvents;

        if (list == null || list.Count == 0)
            return null;

        int index = UnityEngine.Random.Range(0, list.Count);
        return list[index];
    }

    /// <summary>
    /// Indica si hay un evento activo.
    /// </summary>
    public bool HasActiveEvent => activeEvent != null;

    /// <summary>
    /// Evento climático actual.
    /// </summary>
    public ClimateEventData ActiveEvent => activeEvent;
}