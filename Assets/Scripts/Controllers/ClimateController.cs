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
                instance = FindObjectOfType<ClimateController>();

            return instance;
        }
    }

    /// <summary>Evento cuando inicia un evento climático.</summary>
    public event Action<ClimateEventData> OnClimateEventStarted;

    /// <summary>Evento cuando finaliza un evento climático.</summary>
    public event Action<ClimateEventData> OnClimateEventEnded;

    private ClimateProfile    currentProfile;
    private ClimateEventData  activeEvent;

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
    /// Inicializa el sistema climático con el perfil del nivel actual.
    /// </summary>
    public void Initialize(ClimateProfile profile)
    {
        currentProfile = profile;

        if (climateRoutine != null)
            StopCoroutine(climateRoutine);

        climateRoutine = StartCoroutine(ClimateLoop());
    }

    /// <summary>
    /// Loop principal que decide cuándo disparar eventos aleatorios.
    /// </summary>
    private IEnumerator ClimateLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(currentProfile.TimeBetweenEvents);

            if (activeEvent != null)
                continue;

            if (UnityEngine.Random.value <= currentProfile.EventProbability)
                StartClimateEvent(GetRandomEvent());
        }
    }

    /// <summary>
    /// Inicia un evento climático. Si ya hay uno activo lo termina primero.
    /// Puede llamarse tanto por el loop automático como por interacción o debug.
    /// </summary>
    public void StartClimateEvent(ClimateEventData eventData)
    {
        if (eventData == null)
            return;

        if (activeEvent != null)
            EndCurrentEvent();

        activeEvent = eventData;

        OnClimateEventStarted?.Invoke(activeEvent);

        ApplyEffects(activeEvent.Effect);

        activeEventRoutine = StartCoroutine(EventDurationRoutine(activeEvent.Duration));
    }

    /// <summary>
    /// Detiene el evento activo antes de que termine su duración natural.
    /// Pensado para debug y para diálogos que puedan cancelar un evento.
    /// </summary>
    public void ForceStopEvent()
    {
        if (activeEventRoutine != null)
        {
            StopCoroutine(activeEventRoutine);
            activeEventRoutine = null;
        }

        EndCurrentEvent();
    }

    private IEnumerator EventDurationRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        EndCurrentEvent();
    }

    private void EndCurrentEvent()
    {
        if (activeEvent == null)
            return;

        ResetEffects(activeEvent.Effect);
        OnClimateEventEnded?.Invoke(activeEvent);
        activeEvent = null;
    }

    private void ApplyEffects(ClimateEffect effect)
    {
        if (effect == null) return;
        Debug.Log($"[Climate] Aplicando efectos: agua={effect.WaterLevelModifier}, luz={effect.LightIntensityModifier}");
    }

    private void ResetEffects(ClimateEffect effect)
    {
        if (effect == null) return;
        Debug.Log("[Climate] Efectos reseteados.");
    }

    private ClimateEventData GetRandomEvent()
    {
        var list = currentProfile.PossibleEvents;

        if (list == null || list.Count == 0)
            return null;

        return list[UnityEngine.Random.Range(0, list.Count)];
    }

    /// <summary>Indica si hay un evento activo actualmente.</summary>
    public bool HasActiveEvent => activeEvent != null;

    /// <summary>Referencia al evento climático en curso.</summary>
    public ClimateEventData ActiveEvent => activeEvent;
}