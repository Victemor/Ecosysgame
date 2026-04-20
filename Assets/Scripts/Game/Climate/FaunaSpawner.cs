using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Genera fauna en el mundo en respuesta a eventos climáticos.
/// </summary>
public class FaunaSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]

    [SerializeField, Tooltip("Prefab de fauna a instanciar.")]
    private GameObject faunaPrefab;

    [SerializeField, Tooltip("Cantidad base de fauna.")]
    private int baseSpawnCount = 5;

    [SerializeField, Tooltip("Área de spawn.")]
    private Vector3 spawnAreaSize = new Vector3(10, 0, 10);

    private readonly List<GameObject> spawnedFauna = new List<GameObject>();

    private void OnEnable()
    {
        ClimateController.Instance.OnClimateEventStarted += HandleEventStarted;
        ClimateController.Instance.OnClimateEventEnded += HandleEventEnded;
    }

    private void OnDisable()
    {
        ClimateController.Instance.OnClimateEventStarted -= HandleEventStarted;
        ClimateController.Instance.OnClimateEventEnded -= HandleEventEnded;
    }

    /// <summary>
    /// Ajusta spawn al iniciar evento climático.
    /// </summary>
    private void HandleEventStarted(ClimateEventData eventData)
    {
        if (eventData == null || faunaPrefab == null)
            return;

        float modifier = eventData.Effect.FaunaSpawnModifier;

        int spawnAmount = Mathf.RoundToInt(baseSpawnCount * modifier);

        SpawnFauna(spawnAmount);
    }

    /// <summary>
    /// Limpia fauna al finalizar evento.
    /// </summary>
    private void HandleEventEnded(ClimateEventData eventData)
    {
        ClearFauna();
    }

    /// <summary>
    /// Instancia fauna en posiciones aleatorias.
    /// </summary>
    private void SpawnFauna(int amount)
    {
        ClearFauna();

        for (int i = 0; i < amount; i++)
        {
            Vector3 randomPos = transform.position + new Vector3(
                Random.Range(-spawnAreaSize.x, spawnAreaSize.x),
                0,
                Random.Range(-spawnAreaSize.z, spawnAreaSize.z)
            );

            GameObject obj = Instantiate(faunaPrefab, randomPos, Quaternion.identity);
            spawnedFauna.Add(obj);
        }
    }

    /// <summary>
    /// Elimina fauna actual.
    /// </summary>
    private void ClearFauna()
    {
        foreach (var obj in spawnedFauna)
        {
            if (obj != null)
                Destroy(obj);
        }

        spawnedFauna.Clear();
    }
}