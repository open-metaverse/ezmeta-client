using UnityEngine;

/// <summary>
/// ホットキー（ESC、Enter等）の入力を管理するクラス
/// </summary>
public class HotkeyHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private ChatPanel chatPanel;

    private void Update()
    {
        // ESCキーでメニュー開閉
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UIManager.Instance?.ToggleMenu();
        }

        // Enterキーでチャット入力のトグル
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            chatPanel?.ToggleInputFocus();
        }
    }
}
