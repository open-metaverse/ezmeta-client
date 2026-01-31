using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    // プレイヤーはシーンにはないのでシリアライズしない
    private Transform _target; // プレイヤーへの参照

    [SerializeField]
    private Vector3 _offset = new Vector3(0, 5, -7); // カメラのオフセット

    [SerializeField]
    private float _smoothSpeed = 5f; // スムーズさ

    [SerializeField]
    private float _mouseSensitivity = 2f; // マウス感度

    private float _rotationX = 0f;
    private float _rotationY = 0f;

    public void SetTarget(Transform target)
    {
        _target = target;
        // マウス位置を中央に固定
        Cursor.lockState = CursorLockMode.Locked;
    }

    // カメラの回転情報を他スクリプトに提供
    public Quaternion GetCameraRotation()
    {
        // Y軸回転のみを返す（上下回転は移動に影響させない）
        return Quaternion.Euler(0, _rotationX, 0);
    }

    // updateの後に必ず呼ばれる
    private void Update()
    {
        // ESCでゲーム終了
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    private void LateUpdate()
    {
        if (_target == null)
            return;

        // マウス移動で常に視点を回転
        _rotationX += Input.GetAxis("Mouse X") * _mouseSensitivity;
        _rotationY -= Input.GetAxis("Mouse Y") * _mouseSensitivity;
        // 上限を60にすると、反対側に少し回ってしまうので少し制限しないと駄目
        _rotationY = Mathf.Clamp(_rotationY, -30f, 50f);

        // オフセットをマウス入力で回転
        Vector3 rotatedOffset = Quaternion.Euler(_rotationY, _rotationX, 0) * _offset;
        Vector3 desiredPosition = _target.position + rotatedOffset;

        // スムーズに移動
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            _smoothSpeed * Time.deltaTime
        );

        transform.LookAt(_target.position);
    }
}
