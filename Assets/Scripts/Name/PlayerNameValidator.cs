using UnityEngine;

/// <summary>
/// Validador de nombres de jugador.
/// Clase estática — no necesita instancia, solo lógica pura.
/// </summary>
public static class PlayerNameValidator
{
    private const int MaxLength = 15;

    /// <summary>
    /// Indica si la comprobación de nombres prohibidos está activa.
    /// Cambiar a true cuando ForbiddenNamesList tenga su contenido definitivo.
    /// </summary>
    private const bool ForbiddenListActive = false;

    // ── Resultado de validación ───────────────────────────────────────

    public enum ValidationResult
    {
        Valid,
        Empty,
        OnlyWhitespace,
        NoLettersOrDigits,
        TooLong,
        Forbidden
    }

    // ── API pública ───────────────────────────────────────────────────

    /// <summary>
    /// Valida el nombre propuesto y retorna el resultado.
    /// El nombre se limpia (trim) antes de validar.
    /// </summary>
    public static ValidationResult Validate(string name, ForbiddenNamesList forbiddenList = null)
    {
        if (string.IsNullOrEmpty(name))
            return ValidationResult.Empty;

        string trimmed = name.Trim();

        if (trimmed.Length == 0)
            return ValidationResult.OnlyWhitespace;

        if (trimmed.Length > MaxLength)
            return ValidationResult.TooLong;

        if (!ContainsLetterOrDigit(trimmed))
            return ValidationResult.NoLettersOrDigits;

        if (ForbiddenListActive && forbiddenList != null && forbiddenList.Contains(trimmed))
            return ValidationResult.Forbidden;

        return ValidationResult.Valid;
    }

    /// <summary>
    /// Retorna el nombre limpio (trim) si es válido, o null si no lo es.
    /// Conveniente para casos donde solo necesitas saber si pasó o no.
    /// </summary>
    public static string GetValidatedName(string name, ForbiddenNamesList forbiddenList = null)
    {
        string trimmed = name?.Trim();

        return Validate(trimmed, forbiddenList) == ValidationResult.Valid
            ? trimmed
            : null;
    }

    /// <summary>
    /// Convierte el resultado de validación en un mensaje legible para el jugador.
    /// </summary>
    public static string GetErrorMessage(ValidationResult result)
    {
        return result switch
        {
            ValidationResult.Empty            => "El nombre no puede estar vacío.",
            ValidationResult.OnlyWhitespace   => "El nombre no puede ser solo espacios.",
            ValidationResult.NoLettersOrDigits => "El nombre debe contener al menos una letra o número.",
            ValidationResult.TooLong          => $"El nombre no puede superar {MaxLength} caracteres.",
            ValidationResult.Forbidden        => "Este nombre no está permitido.",
            _                                 => string.Empty
        };
    }

    // ── Privados ──────────────────────────────────────────────────────

    /// <summary>
    /// Verifica que el string contenga al menos una letra o dígito.
    /// Rechaza nombres compuestos solo de símbolos, espacios o puntuación.
    /// </summary>
    private static bool ContainsLetterOrDigit(string name)
    {
        foreach (char c in name)
        {
            if (char.IsLetterOrDigit(c))
                return true;
        }

        return false;
    }
}