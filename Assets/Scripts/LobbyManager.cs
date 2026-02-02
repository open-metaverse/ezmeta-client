using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// ロビー管理マネージャー
/// Fusionセッションの開始、プレイヤースポーンを行う
/// </summary>
public class LobbyManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public static LobbyManager Instance { get; private set; }

    [SerializeField]
    private NetworkPrefabRef _playerPrefab;

    [SerializeField]
    private Image _loadingImage;

    [SerializeField]
    private Image _loadingAdImage;

    [SerializeField]
    private Sprite[] _loadingAdSprites;

    [SerializeField]
    private AudioSource _audioSource;

    [SerializeField]
    private AudioClip _loadingSound;

    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters =
        new Dictionary<PlayerRef, NetworkObject>();

    private NetworkRunner _runner;
    private bool _callbacksAdded;
    private bool _isStarting;

    [SerializeField]
    private InputManager _inputManager;

    public void Start()
    {
        if (_loadingImage != null)
        {
            _loadingImage.gameObject.SetActive(false);
        }
        if (_loadingAdImage != null)
        {
            _loadingAdImage.gameObject.SetActive(false);
            _loadingAdImage.preserveAspect = true;
        }

        SetRandomAdSprite();
    }

    /// <summary>
    /// プレイヤー参加時の処理（Fusionコールバック）
    /// </summary>
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            // Create a unique position for the player
            Vector3 spawnPosition = new Vector3(
                (player.RawEncoded % runner.Config.Simulation.PlayerCount) * 3,
                10,
                0
            );
            NetworkObject networkPlayerObject = runner.Spawn(
                _playerPrefab,
                spawnPosition,
                Quaternion.identity,
                player
            );
            // Keep track of the player avatars for easy access
            _spawnedCharacters.Add(player, networkPlayerObject);
        }
    }

    /// <summary>
    /// プレイヤー退出時の処理（Fusionコールバック）
    /// </summary>
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        if (_inputManager == null)
        {
            Debug.LogError("[LobbyManager] InputManager is not assigned.");
            return;
        }

        _inputManager.FillInput(runner, input);
    }

    #region INetworkRunnerCallbacks - 必須の空実装

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
#pragma warning disable UNT0006 // Fusion callback, not Unity message
    public void OnConnectedToServer(NetworkRunner runner) { }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
#pragma warning restore UNT0006
    public void OnConnectRequest(
        NetworkRunner runner,
        NetworkRunnerCallbackArgs.ConnectRequest request,
        byte[] token
    ) { }

    public void OnConnectFailed(
        NetworkRunner runner,
        NetAddress remoteAddress,
        NetConnectFailedReason reason
    ) { }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

    public void OnSessionListUpdated(
        NetworkRunner runner,
        System.Collections.Generic.List<SessionInfo> sessionList
    ) { }

    public void OnCustomAuthenticationResponse(
        NetworkRunner runner,
        System.Collections.Generic.Dictionary<string, object> data
    ) { }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        // ゲームシーンへのロード完了時にローディングUIと音楽を停止
        if (_loadingImage != null)
        {
            _loadingImage.gameObject.SetActive(false);
        }
        if (_loadingAdImage != null)
        {
            _loadingAdImage.gameObject.SetActive(false);
        }
        if (_audioSource != null && _audioSource.isPlaying)
        {
            _audioSource.Stop();
        }
    }

    public void OnSceneLoadStart(NetworkRunner runner) { }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    public void OnReliableDataReceived(
        NetworkRunner runner,
        PlayerRef player,
        ReliableKey key,
        System.ArraySegment<byte> data
    ) { }

    public void OnReliableDataProgress(
        NetworkRunner runner,
        PlayerRef player,
        ReliableKey key,
        float progress
    ) { }

    #endregion

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Debug.Log("[LobbyManager] Awake called");
        // 必ずルートにないと、親がdestroyされたときに一緒に消える
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
        Debug.Log("[LobbyManager] OnDestroy called");
    }

    /// <summary>
    /// Hostボタン用（Canvasボタンから呼ぶ）
    /// </summary>
    public void OnHostButtonClicked()
    {
        Debug.Log("[LobbyManager] OnHostButtonClicked");
        StartGame(GameMode.Host);
    }

    /// <summary>
    /// Joinボタン用（Canvasボタンから呼ぶ）
    /// </summary>
    public void OnJoinButtonClicked()
    {
        Debug.Log("[LobbyManager] OnJoinButtonClicked");
        StartGame(GameMode.Client);
    }

    async void StartGame(GameMode mode)
    {
        if (_isStarting)
        {
            Debug.LogWarning("[LobbyManager] StartGame is already running.");
            return;
        }

        if (_runner != null && _runner.IsRunning)
        {
            Debug.LogWarning("[LobbyManager] Runner is already running.");
            return;
        }

        _isStarting = true;
        SetRandomAdSprite();
        if (_loadingImage != null)
        {
            _loadingImage.gameObject.SetActive(true);
        }
        if (_loadingAdImage != null)
        {
            _loadingAdImage.gameObject.SetActive(true);
        }
        if (_audioSource != null && _loadingSound != null)
        {
            _audioSource.PlayOneShot(_loadingSound);
        }
        Debug.Log($"[LobbyManager] StartGame called with mode: {mode}");

        try
        {
            NetworkRunner runner = EnsureRunner();
            NetworkSceneManagerDefault sceneManager = EnsureSceneManager();

            // GameSceneのSceneRefを取得
            int buildIndex = SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/GameScene.unity");
            if (buildIndex < 0)
            {
                Debug.LogError(
                    "[LobbyManager] GameScene not found in Build Settings. Check the path and build list."
                );
                return;
            }

            var gameScene = SceneRef.FromIndex(buildIndex);
            if (!gameScene.IsValid)
            {
                Debug.LogError("[LobbyManager] GameScene SceneRef is invalid.");
                return;
            }

            Debug.Log($"[LobbyManager] GameScene build index: {gameScene}");

            // Start or join (depends on gamemode) a session with a specific name
            Debug.Log("[LobbyManager] Starting Fusion...");
            await runner.StartGame(
                new StartGameArgs()
                {
                    GameMode = mode,
                    SessionName = sessionName,
                    Scene = gameScene,
                    SceneManager = sceneManager,
                }
            );
            Debug.Log("[LobbyManager] Fusion started!");
        }
        finally
        {
            _isStarting = false;
        }
    }

    [SerializeField]
    private string sessionName = "TestRoom";

    private NetworkRunner EnsureRunner()
    {
        if (_runner == null)
        {
            _runner = GetComponent<NetworkRunner>();
            if (_runner == null)
            {
                _runner = gameObject.AddComponent<NetworkRunner>();
            }
        }

        if (!_callbacksAdded)
        {
            _runner.AddCallbacks(this);
            _callbacksAdded = true;
        }

        _runner.ProvideInput = true;
        return _runner;
    }

    private NetworkSceneManagerDefault EnsureSceneManager()
    {
        NetworkSceneManagerDefault sceneManager = GetComponent<NetworkSceneManagerDefault>();
        if (sceneManager == null)
        {
            sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();
        }

        return sceneManager;
    }

    private void SetRandomAdSprite()
    {
        if (_loadingAdImage == null)
        {
            return;
        }
        if (_loadingAdSprites == null || _loadingAdSprites.Length == 0)
        {
            return;
        }

        int randomIndex = UnityEngine.Random.Range(0, _loadingAdSprites.Length);
        _loadingAdImage.sprite = _loadingAdSprites[randomIndex];
    }
}
