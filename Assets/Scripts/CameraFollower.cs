using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    // プレイやーはシーンにはないのでシリアライズしないで
    private Transform _target; // プレイヤーへの参照

    [SerializeField]
    private Vector3 _offset = new Vector3(0, 5, -7); // カメラのオフセット

    [SerializeField]
    private float _smoothSpeed = 5f; // スムーズさ

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    private void LateUpdate()
    {
        if (_target == null)
            return;

        Vector3 desiredPosition = _target.position + _offset;
        // lerpで段々と移動する
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            _smoothSpeed * Time.deltaTime
        );

        transform.LookAt(_target.position);
    }
}
