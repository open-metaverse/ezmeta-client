using UnityEngine;

/// <summary>
/// GameSceneのUIを管理するマネージャー（シングルトン）
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Menu Panels")]
    [SerializeField]
    private GameObject slidingMenuPanel;

    [SerializeField]
    private Animator menuAnimator;

    [Header("State")]
    [SerializeField]
    private bool _isMenuOpen = false;

    public bool IsMenuOpen => _isMenuOpen;

    private void Awake()
    {
        // シングルトンパターン
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // メニューパネルを初期状態で非表示に
        if (slidingMenuPanel != null)
        {
            slidingMenuPanel.SetActive(false);
        }
    }

    /// <summary>
    /// メニューの開閉をトグルする
    /// </summary>
    public void ToggleMenu()
    {
        _isMenuOpen = !_isMenuOpen;

        if (slidingMenuPanel != null)
        {
            slidingMenuPanel.SetActive(true);
        }

        if (menuAnimator != null)
        {
            menuAnimator.SetBool("IsOpen", _isMenuOpen);
        }
    }

    /// <summary>
    /// メニューを明示的に開く
    /// </summary>
    public void OpenMenu()
    {
        if (_isMenuOpen)
            return;

        _isMenuOpen = true;

        if (slidingMenuPanel != null)
        {
            slidingMenuPanel.SetActive(true);
        }

        if (menuAnimator != null)
        {
            menuAnimator.SetBool("IsOpen", true);
        }
    }

    /// <summary>
    /// メニューを明示的に閉じる
    /// </summary>
    public void CloseMenu()
    {
        if (!_isMenuOpen)
            return;

        _isMenuOpen = false;

        if (menuAnimator != null)
        {
            menuAnimator.SetBool("IsOpen", false);
        }
    }

    /// <summary>
    /// ゲームに戻るボタン用
    /// </summary>
    public void OnResumeButtonClick()
    {
        CloseMenu();
    }

    /// <summary>
    /// 設定ボタン用（未実装）
    /// </summary>
    public void OnSettingsButtonClick()
    {
        Debug.Log("Settings - 未実装");
    }

    /// <summary>
    /// 切断ボタン用
    /// </summary>
    public void OnDisconnectButtonClick()
    {
        Debug.Log("Disconnect - ロビーに戻る");

        // TODO: Fusion Runnerをシャットダウンしてロビーに戻る処理
        // NetworkRunner runner = FindFirstObjectByType<NetworkRunner>();
        // if (runner != null)
        // {
        //     runner.Shutdown();
        // }
        // SceneManager.LoadScene("LobbyScene");
    }
}
