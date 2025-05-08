using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _offset = new Vector3(0, 0, -10);

    [Header("Position Smoothing")]
    [SerializeField] private float _smoothTime = 0.3f;
    [SerializeField] private Vector2 _clampX = new Vector2(-10f, 10f);
    [SerializeField] private Vector2 _clampY = new Vector2(-10f, 10f);
    private Vector3 _currentVelocity = Vector3.zero;

    [Header("Rotation Smoothing")]
    [SerializeField] private float _maxRotationAngle = 10f;
    [SerializeField] private float _rotationSmoothTime = 0.2f;
    private float _currentRotation = 0f;
    private float _rotationVelocity = 0f;

    private void Start()
    {
        if (_target == null)
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null) _target = player.transform;
            else Debug.LogError("SmoothCameraFollow: target not set and no Player tag found.");
        }
    }

    // LateUpdate, чтобы подстроиться под все движения и анимации игрока
    private void LateUpdate()
    {
        if (_target == null) return;
        UpdatePosition();
        UpdateRotation();
    }

    private void UpdatePosition()
    {
        Vector3 desiredPos = _target.position + _offset;

        // 2) сглаженно двигаем из текущей позиции в нужную
        Vector3 smoothed = Vector3.SmoothDamp(
            transform.position,
            desiredPos,
            ref _currentVelocity,
            _smoothTime
        );

        // 3) жёсткое ограничение по краям
        smoothed.x = Mathf.Clamp(smoothed.x, _clampX.x, _clampX.y);
        smoothed.y = Mathf.Clamp(smoothed.y, _clampY.x, _clampY.y);

        transform.position = smoothed;
    }

    private void UpdateRotation()
    {
        // угол, на который хотим повернуться (влево/вправо от центра экрана)
        float screenCenterX = Screen.width * 0.5f;
        float mouseDeltaX = (Input.mousePosition.x - screenCenterX) / screenCenterX;
        float targetRot = mouseDeltaX * _maxRotationAngle;

        // сглаженный переход угла
        _currentRotation = Mathf.SmoothDampAngle(
            _currentRotation,
            targetRot,
            ref _rotationVelocity,
            _rotationSmoothTime
        );

        transform.rotation = Quaternion.Euler(0f, 0f, _currentRotation);
    }
}
