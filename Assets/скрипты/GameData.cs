using UnityEngine;

/// <summary>
/// Статическое хранилище данных между сценами.
/// Хранит выбранных персонажей и другие глобальные данные игры.
/// Не требует MonoBehaviour — доступно из любого скрипта.
/// </summary>
public static class GameData
{
    // ──────────────────────────────────────────────
    // Выбранные персонажи
    // ──────────────────────────────────────────────

    /// <summary>Индекс выбранного персонажа Игрока 1 (в массиве characters).</summary>
    public static int Player1CharacterIndex { get; set; } = -1;

    /// <summary>Имя выбранного персонажа Игрока 1.</summary>
    public static string Player1CharacterName { get; set; } = string.Empty;

    /// <summary>Индекс выбранного персонажа Игрока 2 (в массиве characters).</summary>
    public static int Player2CharacterIndex { get; set; } = -1;

    /// <summary>Имя выбранного персонажа Игрока 2.</summary>
    public static string Player2CharacterName { get; set; } = string.Empty;

    // ──────────────────────────────────────────────
    // Вспомогательные методы
    // ──────────────────────────────────────────────

    /// <summary>Сброс всех данных (например, при выходе в главное меню).</summary>
    public static void Reset()
    {
        Player1CharacterIndex = -1;
        Player1CharacterName = string.Empty;
        Player2CharacterIndex = -1;
        Player2CharacterName = string.Empty;

        Debug.Log("[GameData] Данные сброшены.");
    }

    /// <summary>Вывести текущие выборы в лог (для отладки).</summary>
    public static void DebugLog()
    {
        Debug.Log($"[GameData] Игрок 1: [{Player1CharacterIndex}] {Player1CharacterName}");
        Debug.Log($"[GameData] Игрок 2: [{Player2CharacterIndex}] {Player2CharacterName}");
    }
}