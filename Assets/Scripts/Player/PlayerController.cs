using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    // Синглтон для сохранения между сценами
    public static PlayerController Instance { get; private set; }

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
        // Если это первый экземпляр — сохраняем и инициализируем
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;

            // Инициализация ссылок
            _rigidbody = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
            _weaponManager = GetComponent<WeaponManager>();
            _playerHealth = GetComponent<PlayerHealth>();
            _pauseMenu = FindObjectOfType<PauseMenu>();
        }
        else
        {
            // Удаляем дубликат
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        // Отписка от события при уничтожении
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Вызывается после загрузки каждой новой сцены
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        MoveToSpawnPoint();
        // Если в новой сцене PauseMenu создаётся позже, можно повторно найти его здесь:
        _pauseMenu = FindObjectOfType<PauseMenu>();
    }

    // Перемещает игрока к объекту с тэгом "SpawnPoint"
    private void MoveToSpawnPoint()
    {
        GameObject spawn = GameObject.FindWithTag("Spawnpoint");
        if (spawn != null)
            transform.position = spawn.transform.position;
    }

    private void FixedUpdate()
    {
        if (_playerHealth.isDead) return;
        Move();
        UpdateAnimation();
    }

    private void Move()
    {
        if (_velocity.magnitude < 0.01f)
        {
            _velocity = Vector2.zero;
        }

        if (_moveInput.magnitude > 0.01f && IsPathBlocked(_moveInput.normalized))
        {
            _velocity = Vector2.zero;
            Debug.Log("Wall hit in move direction");
            return;
        }

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
        float speed = _velocity.magnitude;
        _animator.SetFloat("Speed", speed);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (_playerHealth.isDead) return;
        _moveInput = context.ReadValue<Vector2>();
    }

    public void OnLeftMouse(InputAction.CallbackContext context)
    {
        if (!_pauseMenu.isPaused && !_playerHealth.isDead)
        {
            if (context.started)
            {
                if (_weaponManager.GetCurrentWeaponType() == WeaponType.NoWeapon)
                {
                    _weaponManager.TryPickUpWeapon();
                    Debug.Log("Схвачено оружие");
                }
                else
                {
                    WeaponType current = _weaponManager.GetCurrentWeaponType();
                    if (current == WeaponType.Uzi || current == WeaponType.Rifle)
                    {
                        _weaponManager.StartAutoFire();
                    }
                    else
                    {
                        _animator.SetTrigger("Attack");
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
            }
        }
    }

    public void OnRightMouse(InputAction.CallbackContext context)
    {
        if (!_pauseMenu.isPaused && !_playerHealth.isDead)
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
        RaycastHit2D[] hits = new RaycastHit2D[1];
        int count = _rigidbody.Cast(direction, hits, 0.1f);
        return count > 0 && ((1 << hits[0].collider.gameObject.layer) & _wallLayer) != 0;
    }
}
