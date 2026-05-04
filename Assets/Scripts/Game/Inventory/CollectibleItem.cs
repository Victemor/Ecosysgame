using UnityEngine;

/// <summary>
/// Ítem recolectable en el mundo. Al recogerlo con click derecho
/// se agrega al inventario y el GameObject desaparece de la escena.
/// </summary>
[RequireComponent(typeof(PersistenceId))]
public class CollectibleItem : MonoBehaviour
{
    [SerializeField, Tooltip("Datos del ítem que se agregará al inventario al recogerlo.")]
    private ItemData itemData;

    // ── Estado ───────────────────────────────────────────────────────

    private PersistenceId persistenceId;

    public ItemData ItemData  => itemData;

    /// <summary>ID de persistencia de este coleccionable.</summary>
    public string   PersistId => persistenceId != null ? persistenceId.Id : gameObject.name;

    // ── Unity lifecycle ──────────────────────────────────────────────

    private void Awake()
    {
        persistenceId = GetComponent<PersistenceId>();
    }

    // ── API pública ──────────────────────────────────────────────────

    /// <summary>
    /// Intenta agregar el ítem al inventario.
    /// Si el inventario está lleno, no ocurre nada y el objeto permanece en escena.
    /// Guarda el estado inmediatamente tras una recolección exitosa.
    /// </summary>
    public bool TryCollect()
    {
        if (itemData == null)
        {
            Debug.LogWarning($"[CollectibleItem] '{gameObject.name}' no tiene ItemData asignado.", this);
            return false;
        }

        bool added = InventorySystem.Instance.TryAddItem(itemData);

        if (added)
        {
            gameObject.SetActive(false);
            ProgressManager.Instance.SaveWorldState();
        }
        else
        {
            Debug.Log("[CollectibleItem] Inventario lleno — ítem no recolectado.");
        }

        return added;
    }

    /// <summary>
    /// Desactiva silenciosamente este objeto al cargar una partida donde ya fue recogido.
    /// </summary>
    public void RestoreAsCollected()
    {
        gameObject.SetActive(false);
    }
}