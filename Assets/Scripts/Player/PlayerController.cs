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
    private PlayerHealth _playerHealth;

    private PauseMenu _pauseMenu;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _weaponManager = GetComponent<WeaponManager>();
<<<<<<< HEAD
        _playerHealth = GetComponent<PlayerHealth>();
=======
        _pauseMenu = FindObjectOfType<PauseMenu>();
>>>>>>> main
    }

    private void FixedUpdate()
    {
        if (_playerHealth.isDead) return;
        Move();
        UpdateAnimation();
    }

    private void Move()
    {
        if (_velocity.magnitude < 0.01f) // Порог для предотвращения остаточной скорости
        {
            _velocity = Vector2.zero;
        }
        // Если есть ввод и путь заблокирован в направлении движения
        if (_moveInput.magnitude > 0.01f && IsPathBlocked(_moveInput.normalized))
        {
            _velocity = Vector2.zero;
            Debug.Log("Wall hit in move direction");
            return;
        }

        // Расчет целевой скорости
        Vector2 targetVelocity = _moveInput * _maxSpeed;
        Vector2 velocityDiff = targetVelocity - _velocity;
        float accelerateRate = (targetVelocity.magnitude > 0.01f) ? _acceleration : _deceleration;
        Vector2 movement = velocityDiff * (accelerateRate * Time.fixedDeltaTime);

        _velocity += movement;
        _velocity = Vector2.ClampMagnitude(_velocity, _maxSpeed);
        _velocity *= Mathf.Pow(1f - _velocityPower, Time.fixedDeltaTime);

        _rigidbody.MovePosition(_rigidbody.position + _velocity * Time.fixedDeltaTime);
    }

    private void UpdateAnimation()
    {
        float speed = _velocity.magnitude; // Вычисляем текущую скорость
        _animator.SetFloat("Speed", speed);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (_playerHealth.isDead) return;
        _moveInput = context.ReadValue<Vector2>();
    }

    // ЛКМ: если оружия нет – подбираем, иначе – атакуем
    public void OnLeftMouse(InputAction.CallbackContext context)
    {
<<<<<<< HEAD
        if (_playerHealth.isDead) return;
        if (context.started)
=======
        if (!_pauseMenu.isPaused)
>>>>>>> main
        {
            if (context.started)
            {
                if (_weaponManager.GetCurrentWeaponType() == WeaponType.NoWeapon)
                {
                    _weaponManager.TryPickUpWeapon();
                    Debug.Log("Пробуем подобрать оружие");
                }
                else
                {
                    WeaponType current = _weaponManager.GetCurrentWeaponType();
                    // Если оружие имеет автоматический огонь (Uzi или Rifle), запускаем автострельбу
                    if (current == WeaponType.Uzi || current == WeaponType.Rifle)
                    {
                        _weaponManager.StartAutoFire();
                        //Debug.Log("Запуск автострельбы");
                    }
                    else
                    {
                        // Для остальных оружий запускаем одиночный выстрел через анимацию
                        _animator.SetTrigger("Attack");
                        //Debug.Log("Запуск анимации атаки");
                    }
                }
            }
        }
        if (context.canceled)
        {
            WeaponType current = _weaponManager.GetCurrentWeaponType();
            if (current == WeaponType.Uzi || current == WeaponType.Rifle)
            {
                _weaponManager.StopAutoFire();
                //Debug.Log("Остановка автострельбы");
            }
        }
    }

    // ПКМ: сброс оружия (если оно есть)
    public void OnRightMouse(InputAction.CallbackContext context)
    {
<<<<<<< HEAD
        if (_playerHealth.isDead) return;
        if (context.performed)
=======
        if (!_pauseMenu.isPaused)
>>>>>>> main
        {
            if (context.performed)
            {
                if (_weaponManager.GetCurrentWeaponType() != WeaponType.NoWeapon)
                {
                    _weaponManager.DropWeapon();
                }
            }
        }
    }

    public Vector2 GetMoveInput()
    {
        return _moveInput;
    }

    private bool IsPathBlocked(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(_rigidbody.position, direction, 0.4f, _wallLayer);
        // Debug.DrawRay(_rigidbody.position, direction * 0.4f, Color.red); // Визуализация луча
        return hit.collider != null;
    }
}