using UnityEngine;
using Fusion;

public class NetworkLauncher : MonoBehaviour
{
    private NetworkRunner _runner;

    // Назначь этот метод на OnClick() твоей кнопки "Искать бой"
    public async void StartMatchmaking()
    {
        _runner = gameObject.GetComponent<NetworkRunner>();
        if (_runner == null)
        {
            _runner = gameObject.AddComponent<NetworkRunner>();
        }

        _runner.ProvideInput = true;

        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = "FightRoom_1v1",
            PlayerCount = 2,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }
}