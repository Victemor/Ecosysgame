using UnityEngine;

/// <summary>
/// Datos de un ítem del inventario.
/// Contiene únicamente información — sin lógica de ejecución.
/// </summary>
[CreateAssetMenu(menuName = "Game/Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Identificación")]

    [SerializeField, Tooltip("ID único del ítem. Usado por el sistema de guardado para identificarlo. " +
                             "Debe coincidir con el ID registrado en ItemDatabase.")]
    private string id;

    [SerializeField, Tooltip("Nombre visible del ítem en la UI.")]
    private string itemName;

    [SerializeField, Tooltip("Ícono representativo del ítem.")]
    private Sprite icon;

    public string Id       => id;
    public string ItemName => itemName;
    public Sprite Icon     => icon;
}