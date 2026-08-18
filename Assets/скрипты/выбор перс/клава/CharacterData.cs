using UnityEngine;

// ============================================================
// Статический "мост" между сценой выбора персонажа и игровой сценой.
// Не вешается ни на какой GameObject — просто держит данные,
// пока Unity не выгрузит домен приложения.
// ============================================================
public static class GameData1
{
    // Индекс и имя (как было раньше)
    public static int Player1CharacterIndex1;
    public static string Player1CharacterName1;

    public static int Player2CharacterIndex1;
    public static string Player2CharacterName1;

    // НОВОЕ: сами префабы персонажей (куб, круг и т.д.),
    // которые надо заспавнить в игровой сцене
    public static GameObject Player1CharacterPrefab1;
    public static GameObject Player2CharacterPrefab1;
}