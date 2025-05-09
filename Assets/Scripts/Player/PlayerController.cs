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
    [SerializeField] private LayerMask _wallLayer;
    [SerializeField] private float _acceleration = 50f;
    [SerializeField] private float _deceleration = 50f;

    private Vector2 _moveInput;
    private Rigidbody2D _rigidbody;
    private Vector2 _velocity;
    private ContactFilter2D _castFilter;
    private RaycastHit2D[] _hitBuffer = new RaycastHit2D[4];

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

        _castFilter = new ContactFilter2D();
        _castFilter.useLayerMask = true;
        _castFilter.layerMask = _wallLayer;
        _castFilter.useTriggers = false;
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
        float delta = Time.fixedDeltaTime;

        Vector2 targetVelocity = (_moveInput.sqrMagnitude > 0.01f)
            ? _moveInput.normalized * _maxSpeed
            : Vector2.zero;

        float accelRate = (_moveInput.sqrMagnitude > 0.01f)
            ? _acceleration
            : _deceleration;

        _velocity = Vector2.MoveTowards(_velocity, targetVelocity, accelRate * delta);

        // Проверяем столкновения по каждой оси и обнуляем компоненту скорости
        if (Mathf.Abs(_velocity.x) > 0.001f)
        {
            Vector2 dirX = new Vector2(Mathf.Sign(_velocity.x), 0f);
            float distX = Mathf.Abs(_velocity.x * delta);
            if (_rigidbody.Cast(dirX, _castFilter, _hitBuffer, distX) > 0)
                _velocity.x = 0;
        }

        // вертикаль
        if (Mathf.Abs(_velocity.y) > 0.001f)
        {
            Vector2 dirY = new Vector2(0f, Mathf.Sign(_velocity.y));
            float distY = Mathf.Abs(_velocity.y * delta);
            if (_rigidbody.Cast(dirY, _castFilter, _hitBuffer, distY) > 0)
                _velocity.y = 0;
        }

        _rigidbody.velocity = _velocity;
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

    // Обработка левого клика мыши: подобрать оружие или атаковать
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
                    // Для автоматического оружия запускаем автострельбу
                    if (current == WeaponType.Uzi || current == WeaponType.Rifle)
                    {
                        _weaponManager.StartAutoFire();
                    }
                    else
                    {
                        // Одиночная атака
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

    // private bool IsPathBlocked(Vector2 direction)
    // {
    //     RaycastHit2D hit = Physics2D.Raycast(_rigidbody.position, direction, 0.4f, _wallLayer);
    //     // Debug.DrawRay(_rigidbody.position, direction * 0.4f, Color.red); // ������������ ����
    //     return hit.collider != null;
    // }

}
