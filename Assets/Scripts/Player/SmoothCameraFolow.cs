using UnityEngine;

public class SmoothCameraFolow : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _smoothTime = 0.3f;
    [SerializeField] private float _rotationSpeed = 5f;
    [SerializeField] private float _maxRotationAngle = 10f;
    [SerializeField] private Vector3 _offset = new Vector3(0, 0, -10); // �������� ������ ������������ ����

    private Vector3 _velocity = Vector3.zero;
    private float _currentRotation = 0f;
    private GameObject _player; //----------

    private void Start()
    {
        _player = GameObject.FindWithTag("Player");
        if (_player != null)
        {
            _target = _player.transform;
        }
        else
        {
            Debug.LogError("�� ������ ������ � ����� 'Player' � �����!");
        }
    }

    private void FixedUpdate()
    {
        if (_target == null)
        {
            Debug.LogWarning("������ �� ����������!");
            return;
        }

        FollowTarget();
        RotateCamera();
    }

    private void FollowTarget()
    {
        // �������� ���� � ��������� ��������
        Vector3 targetPosition = _target.position + _offset;

        // ���������� �������� ������, ����� �������� ������ ���������
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref _velocity, _smoothTime);

        // ���� ����� �� ���������, �� ��������� �������� ������
        if (_target.GetComponent<Rigidbody2D>().velocity.magnitude < 0.1f)
        {
            smoothedPosition = Vector3.Lerp(transform.position, smoothedPosition, 0.1f); // �������������� �����������, ����� ����� �� ���������
        }

        // ������������� ������ � ������������� �� ��� X � Y
        smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, -10f, 10f);  // ������ �����������, ���������� �� ������
        smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, -10f, 10f);

        // ��������� ������� � ������
        transform.position = smoothedPosition;
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
