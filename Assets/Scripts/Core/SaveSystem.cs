using System.IO;
using UnityEngine;

/// <summary>
/// Escribe y lee el archivo de guardado en Application.persistentDataPath.
/// Opera solo con datos serializables — no conoce ningún sistema de juego.
/// </summary>
public static class SaveSystem
{
    private static string SavePath => 
        Path.Combine(Application.persistentDataPath, "savegame.json");

    /// <summary>
    /// Guarda el estado completo del juego en disco.
    /// Sobreescribe el archivo anterior.
    /// </summary>
    public static void Save(GameSaveData data)
    {
        string json = JsonUtility.ToJson(data, prettyPrint: true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"[SaveSystem] Guardado en: {SavePath}");
    }

    /// <summary>
    /// Carga el estado desde disco.
    /// Retorna null si no existe archivo de guardado.
    /// </summary>
    public static GameSaveData Load()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("[SaveSystem] No existe archivo de guardado — primera partida.");
            return null;
        }

        string json = File.ReadAllText(SavePath);
        return JsonUtility.FromJson<GameSaveData>(json);
    }

    /// <summary>
    /// Elimina el archivo de guardado.
    /// Usado por el reset de progreso.
    /// </summary>
    public static void DeleteSave()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }
}