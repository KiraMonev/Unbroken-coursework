using UnityEngine;

public class SmoothCameraFolow : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _smoothTime = 0.3f;
    [SerializeField] private float _rotationSpeed = 5f;
    [SerializeField] private float _maxRotationAngle = 10f;
    [SerializeField] private Vector3 _offset = new Vector3(0, 0, -10); // Смещение камеры относительно цели

    private Vector3 _velocity = Vector3.zero;
    private float _currentRotation = 0f;

    private void LateUpdate()
    {
        if (_target == null)
        {
            Debug.LogWarning("Таргет не установлен!");
            return;
        }

        FollowTarget();
        RotateCamera();
    }

    private void FollowTarget()
    {
        Vector3 targetPosition = _target.position + _offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _velocity, _smoothTime);
    }

    private void RotateCamera()
    {
        Vector3 mousePosition = Input.mousePosition;
        float screenCenterX = Screen.width / 2f;
        float mouseDeltaX = (mousePosition.x - screenCenterX) / screenCenterX;

        float targetRotation = mouseDeltaX * _maxRotationAngle;
        _currentRotation = Mathf.Lerp(_currentRotation, targetRotation, Time.deltaTime * _rotationSpeed);

        transform.rotation = Quaternion.Euler(0, 0, _currentRotation);
    }
}
