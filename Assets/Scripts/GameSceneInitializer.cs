using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameSceneの初期化。LobbySceneから来ていない場合はLobbySceneに戻す
/// </summary>
public class GameSceneInitializer : MonoBehaviour
{
    private const string LOBBY_SCENE = "LobbyScene";

    private void Start()
    {
        // LobbyManagerが存在するかチェック（DontDestroyOnLoadでも探せる）
        LobbyManager lobbyManager = FindFirstObjectByType<LobbyManager>();

        // LobbyManagerがない場合はLobbySceneに戻る
        if (lobbyManager == null)
        {
            Debug.Log("[GameSceneInitializer] LobbyManager not found. Loading LobbyScene...");
            SceneManager.LoadScene(LOBBY_SCENE);
        }
        else
        {
            Debug.Log("[GameSceneInitializer] LobbyManager found. Game started!");
        }
    }
}
