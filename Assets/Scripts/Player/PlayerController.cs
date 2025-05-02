using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _maxSpeed = 5f;
    [SerializeField] private float _acceleration = 50f;
    [SerializeField] private float _deceleration = 50f;
    [SerializeField] private float _velocityPower = 0.9f;
    [SerializeField] private LayerMask _wallLayer;

    private Vector2 _moveInput;
    private Rigidbody2D _rigidbody;
    private Vector2 _velocity;

    [Header("References")]
    private WeaponManager _weaponManager;
    private Animator _animator;


    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _weaponManager = GetComponent<WeaponManager>();
    }

    private void FixedUpdate()
    {
        Move();
        UpdateAnimation();
    }

    // private void Move()
    // {
    //     if (_velocity.magnitude < 0.01f) // ����� ��� �������������� ���������� ��������
    //     {
    //         _velocity = Vector2.zero;
    //     }
    //     // ���� ���� ���� � ���� ������������ � ����������� ��������
    //     if (_moveInput.magnitude > 0.01f && IsPathBlocked(_moveInput.normalized))
    //     {
    //         _velocity = Vector2.zero;
    //         Debug.Log("Wall hit in move direction");
    //         return;
    //     }

    //     // ������ ������� ��������
    //     Vector2 targetVelocity = _moveInput * _maxSpeed;
    //     Vector2 velocityDiff = targetVelocity - _velocity;
    //     float accelerateRate = (targetVelocity.magnitude > 0.01f) ? _acceleration : _deceleration;
    //     Vector2 movement = velocityDiff * (accelerateRate * Time.fixedDeltaTime);

    //     _velocity += movement;
    //     _velocity = Vector2.ClampMagnitude(_velocity, _maxSpeed);
    //     _velocity *= Mathf.Pow(1f - _velocityPower, Time.fixedDeltaTime);

    //     _rigidbody.MovePosition(_rigidbody.position + _velocity * Time.fixedDeltaTime);
    // }

    private void Move()
    {
        Vector2 targetVelocity = _moveInput * _maxSpeed;

        Vector2 velocityDiff = targetVelocity - _velocity;
        float accelerateRate = (_moveInput.magnitude > 0.01f) ? _acceleration : _deceleration;
        Vector2 movement = velocityDiff * (accelerateRate * Time.fixedDeltaTime);

        _velocity += movement;
        _velocity = Vector2.ClampMagnitude(_velocity, _maxSpeed);
        _velocity *= Mathf.Pow(1f - _velocityPower, Time.fixedDeltaTime);

        Vector2 proposedPosition = _rigidbody.position + _velocity * Time.fixedDeltaTime;

        if (!IsPathBlocked(proposedPosition))
        {
            _rigidbody.MovePosition(proposedPosition);
        }
        else
        {
            _velocity = Vector2.zero;
            Debug.Log("Blocked by wall.");
        }
    }

    private void UpdateAnimation()
    {
        float speed = _velocity.magnitude; // ��������� ������� ��������
        _animator.SetFloat("Speed", speed);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    // ���: ���� ������ ��� � ���������, ����� � �������
    public void OnLeftMouse(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (_weaponManager.GetCurrentWeaponType() == WeaponType.NoWeapon)
            {
                _weaponManager.TryPickUpWeapon();
                Debug.Log("������� ��������� ������");
            }
            else
            {
                WeaponType current = _weaponManager.GetCurrentWeaponType();
                // ���� ������ ����� �������������� ����� (Uzi ��� Rifle), ��������� ������������
                if (current == WeaponType.Uzi || current == WeaponType.Rifle)
                {
                    _weaponManager.StartAutoFire();
                    //Debug.Log("������ ������������");
                }
                else
                {
                    // ��� ��������� ������ ��������� ��������� ������� ����� ��������
                    _animator.SetTrigger("Attack");
                    //Debug.Log("������ �������� �����");
                }
            }
        }

        if (context.canceled)
        {
            WeaponType current = _weaponManager.GetCurrentWeaponType();
            if (current == WeaponType.Uzi || current == WeaponType.Rifle)
            {
                _weaponManager.StopAutoFire();
                //Debug.Log("��������� ������������");
            }
        }
    }

    // ���: ����� ������ (���� ��� ����)
    public void OnRightMouse(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (_weaponManager.GetCurrentWeaponType() != WeaponType.NoWeapon)
            {
                _weaponManager.DropWeapon();
            }
        }
    }

    public Vector2 GetMoveInput()
    {
        return _moveInput;
    }

    // private bool IsPathBlocked(Vector2 direction)
    // {
    //     RaycastHit2D hit = Physics2D.Raycast(_rigidbody.position, direction, 0.4f, _wallLayer);
    //     // Debug.DrawRay(_rigidbody.position, direction * 0.4f, Color.red); // ������������ ����
    //     return hit.collider != null;
    // }

    private bool IsPathBlocked(Vector2 targetPosition)
    {
        Collider2D collider = GetComponent<BoxCollider2D>();
        if (collider == null) return false;

        Vector2 offset = targetPosition - _rigidbody.position;

        RaycastHit2D hit = Physics2D.BoxCast(_rigidbody.position, collider.bounds.size, 0f, offset.normalized, offset.magnitude, _wallLayer);

        return hit.collider != null;
    }

}
