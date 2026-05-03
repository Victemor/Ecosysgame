using UnityEngine;

/// <summary>
/// Datos de una página de la bitácora.
/// Cada página tiene nombre, dos descripciones y una imagen.
/// Se crea como asset desde el menú contextual de Assets.
/// </summary>
[CreateAssetMenu(fileName = "BitacoraPage", menuName = "Ecosysgame/Bitacora/Pagina")]
public class BitacoraPageData : ScriptableObject
{
    [Header("Contenido")]

    [Tooltip("Nombre principal de la página.")]
    public string nombre;

    [Tooltip("Primera descripción o texto de la página.")]
    [TextArea(2, 4)]
    public string descripcion1;

    [Tooltip("Segunda descripción o texto adicional.")]
    [TextArea(2, 4)]
    public string descripcion2;

    [Tooltip("Imagen representativa de la página.")]
    public Sprite imagen;
}