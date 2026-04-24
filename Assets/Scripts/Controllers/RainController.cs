using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sistema de lluvia basado en partículas que reacciona a eventos climáticos.
/// Genera efectos de colisión (splashes) usando object pooling para evitar
/// instanciaciones en runtime. Soporta transiciones suaves de intensidad.
///
/// REQUISITOS DE ESCENA:
/// - Requiere un ParticleSystem en el mismo GameObject con "Collision" habilitado.
/// - El splashPrefab debe ser un objeto ligero (sprite o efecto pequeño).
/// - ClimateController debe existir en escena para recibir eventos.
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class RainController : MonoBehaviour
{
    #region Serialized Fields

    [Header("Splash")]

    [SerializeField, Tooltip("Prefab visual que se instancia al colisionar la lluvia con una superficie.")]
    private GameObject splashPrefab;

    [SerializeField, Min(0.1f), Tooltip("Tiempo en segundos que el splash permanece activo antes de volver al pool.")]
    private float splashLifetime = 1.5f;

    [Header("Emission")]

    [SerializeField, Min(0), Tooltip("Partículas por segundo en estado de reposo (antes de cualquier evento). Normalmente 0.")]
    private float defaultEmissionRate = 0f;

    [SerializeField, Min(0.01f), Tooltip("Duración en segundos de la transición de entrada y salida de la lluvia.")]
    private float transitionDuration = 2f;

    [Header("Pool")]

    [SerializeField, Min(1), Tooltip("Tamaño mínimo del pool de splashes aunque la intensidad sea baja.")]
    private int minPoolSize = 10;

    #endregion

    #region Private Fields

    private ParticleSystem rainParticleSystem;
    private readonly List<GameObject>            splashPool      = new List<GameObject>();
    private          List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

    private int      poolIndex;
    private float    currentRate;
    private Coroutine transitionRoutine;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        EnsureInitialized();
    }

    private void Start()
    {
        SetEmissionRate(defaultEmissionRate);
        InitializePool(defaultEmissionRate);
    }

    private void OnEnable()
    {
        ClimateController.Instance.OnClimateEventStarted += HandleEventStarted;
        ClimateController.Instance.OnClimateEventEnded   += HandleEventEnded;
    }

    private void OnDisable()
    {
        ClimateController.Instance.OnClimateEventStarted -= HandleEventStarted;
        ClimateController.Instance.OnClimateEventEnded   -= HandleEventEnded;
    }

    private void OnParticleCollision(GameObject other)
    {
        EnsureInitialized();

        int numEvents = rainParticleSystem.GetCollisionEvents(other, collisionEvents);

        for (int i = 0; i < numEvents; i++)
        {
            Vector3    pos    = collisionEvents[i].intersection;
            GameObject splash = GetSplashFromPool();

            if (splash == null)
                continue;

            splash.transform.position = pos;
            splash.SetActive(true);
            StartCoroutine(ReturnToPoolAfterSeconds(splash, splashLifetime));
        }
    }

    #endregion

    #region Climate Event Handlers

    /// <summary>
    /// Reacciona al inicio de un evento climático activando la lluvia
    /// con la intensidad definida en ClimateEffect.RainIntensity.
    /// </summary>
    private void HandleEventStarted(ClimateEventData eventData)
    {
        if (eventData?.Effect == null)
            return;

        float targetRate = eventData.Effect.RainIntensity;

        if (targetRate > 0f)
            TransitionEmission(targetRate, transitionDuration);
    }

    /// <summary>
    /// Reacciona al fin de un evento climático deteniendo la lluvia suavemente.
    /// </summary>
    private void HandleEventEnded(ClimateEventData eventData)
    {
        StopRain(transitionDuration);
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Garantiza que todas las referencias críticas estén inicializadas.
    /// Puede ser llamado de forma segura antes de Start().
    /// </summary>
    private void EnsureInitialized()
    {
        if (rainParticleSystem == null)
            rainParticleSystem = GetComponent<ParticleSystem>();

        if (collisionEvents == null)
            collisionEvents = new List<ParticleCollisionEvent>();
    }

    #endregion

    #region Object Pool

    /// <summary>
    /// Asegura que el pool tenga al menos los objetos necesarios para la tasa dada.
    /// Solo instancia objetos nuevos; nunca destruye los existentes.
    /// El factor x2 sobre la tasa es un heurístico para que el pool raramente se agote.
    /// </summary>
    private void InitializePool(float targetRate)
    {
        if (splashPrefab == null)
        {
            Debug.LogWarning("[Rain] splashPrefab no asignado. El pool no puede crearse.", this);
            return;
        }

        int desiredSize = Mathf.Max(minPoolSize, Mathf.CeilToInt(targetRate * 2f));

        for (int i = splashPool.Count; i < desiredSize; i++)
        {
            GameObject obj = Instantiate(splashPrefab, transform);
            obj.SetActive(false);
            splashPool.Add(obj);
        }
    }

    /// <summary>
    /// Devuelve el siguiente splash disponible del pool usando rotación circular.
    /// Si todos están activos, fuerza el retorno del siguiente en cola (fallback).
    /// </summary>
    private GameObject GetSplashFromPool()
    {
        int count = splashPool.Count;

        if (count == 0)
        {
            Debug.LogWarning("[Rain] Pool vacío. Asigna un splashPrefab y configura el pool.", this);
            return null;
        }

        for (int i = 0; i < count; i++)
        {
            int index = (poolIndex + i) % count;

            if (splashPool[index] != null && !splashPool[index].activeInHierarchy)
            {
                poolIndex = (index + 1) % count;
                return splashPool[index];
            }
        }

        // Fallback: reutiliza el siguiente aunque esté activo (evita cortar el ciclo de juego)
        int fallback = poolIndex % count;
        poolIndex = (fallback + 1) % count;
        return splashPool[fallback];
    }

    #endregion

    #region Emission Control

    /// <summary>
    /// Aplica directamente la tasa de emisión sin transición.
    /// </summary>
    private void SetEmissionRate(float value)
    {
        EnsureInitialized();

        if (rainParticleSystem == null)
            return;

        var emission = rainParticleSystem.emission;
        emission.rateOverTime = value;
        currentRate = value;
    }

    /// <summary>
    /// Inicia una transición suave de la tasa de emisión.
    /// Cancela cualquier transición en curso antes de iniciar la nueva.
    /// </summary>
    private void TransitionEmission(float targetRate, float duration)
    {
        EnsureInitialized();

        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        float safeDuration = Mathf.Max(duration, 0.01f);

        InitializePool(targetRate);

        transitionRoutine = StartCoroutine(EmissionTransitionRoutine(targetRate, safeDuration));
    }

    /// <summary>
    /// Detiene la lluvia con transición suave hacia emisión cero.
    /// </summary>
    public void StopRain(float duration = 2f)
    {
        TransitionEmission(0f, duration);
    }

    private IEnumerator EmissionTransitionRoutine(float targetRate, float duration)
    {
        float startRate = currentRate;
        float elapsed   = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // Ease-out cuadrático: arranque rápido, llegada suave
            float t       = elapsed / duration;
            float easedT  = 1f - (1f - t) * (1f - t);
            float newRate = Mathf.Lerp(startRate, targetRate, easedT);

            SetEmissionRate(newRate);
            yield return null;
        }

        SetEmissionRate(targetRate);
        transitionRoutine = null;
    }

    #endregion

    #region Utilities

    private IEnumerator ReturnToPoolAfterSeconds(GameObject obj, float seconds)
    {
        yield return new WaitForSeconds(seconds);

        if (obj != null)
            obj.SetActive(false);
    }

    #endregion
}