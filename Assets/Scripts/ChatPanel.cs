using System;
using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// チャットUIパネル。複数メッセージのキュー管理と表示を行う
/// </summary>
public class ChatPanel : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField]
    private TMP_Text chatText;

    [SerializeField]
    private TMP_InputField inputField;

    [SerializeField]
    private ScrollRect scrollRect;

    [SerializeField]
    private Button sendButton;

    [Header("Settings")]
    [SerializeField]
    private int maxMessages = 50;

    private Queue<string> _messageQueue = new();
    private bool _isInputFocused = false;

    private void Start()
    {
        // 送信ボタンにリスナーを追加
        if (sendButton != null)
        {
            sendButton.onClick.AddListener(SendMessage);
        }

        // 入力フィールドのリスナー
        if (inputField != null)
        {
            inputField.onEndEdit.AddListener(OnInputFieldEndEdit);
        }
    }

    /// <summary>
    /// メッセージを追加して表示を更新
    /// </summary>
    public void AddMessage(string message)
    {
        _messageQueue.Enqueue(message);

        // 最大数を超えたら古いメッセージを削除
        if (_messageQueue.Count > maxMessages)
        {
            _messageQueue.Dequeue();
        }

        // テキストを更新
        UpdateChatDisplay();
    }

    /// <summary>
    /// タイムスタンプ付きでメッセージを追加
    /// </summary>
    public void AddMessage(string message, PlayerRef messageSource, PlayerRef localPlayer)
    {
        string timeStamp = DateTime.Now.ToString("HH:mm:ss");

        if (messageSource == localPlayer)
        {
            message = $"[{timeStamp}] You: {message}";
        }
        else
        {
            message = $"[{timeStamp}] Player {messageSource.PlayerId}: {message}";
        }

        AddMessage(message);
    }

    /// <summary>
    /// チャット表示を更新して自動スクロール
    /// </summary>
    private void UpdateChatDisplay()
    {
        if (chatText == null)
            return;

        // キューから文字列を生成
        chatText.text = string.Join("\n", _messageQueue);

        // 自動スクロール
        if (scrollRect != null)
        {
            StartCoroutine(ScrollToBottom());
        }
    }

    /// <summary>
    /// チャットの最下部へスクロール
    /// </summary>
    private System.Collections.IEnumerator ScrollToBottom()
    {
        // レイアウトの更新を待ってからスクロール
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    /// <summary>
    /// チャットメッセージを送信
    /// </summary>
    public void SendMessage()
    {
        if (inputField == null || string.IsNullOrWhiteSpace(inputField.text))
            return;

        string message = inputField.text.Trim();

        // PlayerからRPCを呼び出し
        Player localPlayer = FindFirstObjectByType<Player>();
        if (localPlayer != null)
        {
            localPlayer.RPC_SendMessage(message);
        }

        // 入力フィールドをクリア
        inputField.text = "";
        inputField.DeactivateInputField();
        _isInputFocused = false;
    }

    /// <summary>
    /// 入力フィールドのフォーカスが外れた時の処理
    /// </summary>
    private void OnInputFieldEndEdit(string text)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SendMessage();
        }
    }

    /// <summary>
    /// 入力フィールドのフォーカスをトグル
    /// </summary>
    public void ToggleInputFocus()
    {
        if (inputField == null)
            return;

        _isInputFocused = !_isInputFocused;

        if (_isInputFocused)
        {
            inputField.ActivateInputField();
        }
        else
        {
            inputField.DeactivateInputField();
        }
    }

    /// <summary>
    /// チャットをクリア
    /// </summary>
    public void ClearChat()
    {
        _messageQueue.Clear();
        UpdateChatDisplay();
    }
}
