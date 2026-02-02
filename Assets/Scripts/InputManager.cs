using Fusion;
using UnityEngine;

/// <summary>
/// 入力処理専用マネージャー
/// Fusionの入力データを生成する
/// </summary>
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private bool _mouseButton0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        _mouseButton0 |= Input.GetMouseButton(0);
    }

    public void FillInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();

        var direction = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
            direction += Vector3.forward;

        if (Input.GetKey(KeyCode.S))
            direction += Vector3.back;

        if (Input.GetKey(KeyCode.A))
            direction += Vector3.left;

        if (Input.GetKey(KeyCode.D))
            direction += Vector3.right;

        if (direction.sqrMagnitude > 0)
        {
            direction.Normalize();
            CameraFollower camera = FindFirstObjectByType<CameraFollower>();
            if (camera != null)
            {
                direction = camera.GetCameraRotation() * direction;
            }
        }

        data.direction = direction;
        data.buttons.Set(NetworkInputData.MOUSEBUTTON0, _mouseButton0);
        _mouseButton0 = false;

        input.Set(data);
    }
}
