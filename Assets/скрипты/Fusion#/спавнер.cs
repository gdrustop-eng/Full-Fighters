using UnityEngine;
using Fusion;

public class CharacterSpawner : SimulationBehaviour
{
    // Сюда перетаскиваем префаб "кругг"
    public NetworkObject playerPrefab;

    public void SpawnPlayer(NetworkRunner runner, PlayerRef player)
    {
        if (player == runner.LocalPlayer)
        {
            // Позиция: 1-й игрок слева (-3), 2-й справа (3)
            Vector3 spawnPosition = (player.RawEncoded % 2 == 0) ? new Vector3(-3, 0, 0) : new Vector3(3, 0, 0);

            // Создаем игрока в сети
            runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player);
        }
    }
}