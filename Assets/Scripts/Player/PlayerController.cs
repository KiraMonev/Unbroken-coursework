using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Настройки движения")]
    [SerializeField] private float _maxSpeed = 5f;
    [SerializeField] private LayerMask _wallLayer;
    [SerializeField] private float _acceleration = 50f;
    [SerializeField] private float _deceleration = 50f;

    [Header("Настройки рывка")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;

    private bool isDashing;
    private float dashTimeLeft;
    private float dashCooldownTimer;
    private Vector2 dashDirection;
    [SerializeField] private bool canDash = false; 

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
    public bool isConversation = false;

    private string _currentScene;

    private Shop _shop;

    private void Awake()
    {
        // Если это первый экземпляр — сохраняем и инициализируем
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;

            _rigidbody = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
            _weaponManager = GetComponent<WeaponManager>();
            _playerHealth = GetComponent<PlayerHealth>();
            _pauseMenu = FindObjectOfType<PauseMenu>();
            _shop = FindObjectOfType<Shop>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Вызывается после загрузки каждой новой сцены
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

        if (scene.name == "MainMenu")
        {
            Destroy(gameObject);
            return;
        }

        MoveToSpawnPoint();
        _velocity.x = 0;
        _velocity.y = 0;
        // Если в новой сцене PauseMenu создаётся позже, можно повторно найти его здесь:
        _pauseMenu = FindObjectOfType<PauseMenu>();
        _shop = FindObjectOfType<Shop>();

        _castFilter = new ContactFilter2D();
        _castFilter.useLayerMask = true;
        _castFilter.layerMask = _wallLayer;
        _castFilter.useTriggers = false;
        _currentScene = SceneManager.GetActiveScene().name;
    }

    private void MoveToSpawnPoint()
    {
        GameObject spawn = GameObject.FindWithTag("Spawnpoint");
        if (spawn != null)
            transform.position = spawn.transform.position;
    }

    private void FixedUpdate()
    {
        if (_playerHealth.isDead || isConversation) return;

        if (dashCooldownTimer > 0)
            dashCooldownTimer -= Time.fixedDeltaTime;

        if (isDashing)
            PerformDash();
        else
        {
            Move();
        }
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

        // Проверка столкновений по каждой оси
        if (Mathf.Abs(_velocity.x) > 0.001f)
        {
            Vector2 dirX = new Vector2(Mathf.Sign(_velocity.x), 0f);
            float distX = Mathf.Abs(_velocity.x * delta);
            if (_rigidbody.Cast(dirX, _castFilter, _hitBuffer, distX) > 0)
                _velocity.x = 0;
        }

        if (Mathf.Abs(_velocity.y) > 0.001f)
        {
            Vector2 dirY = new Vector2(0f, Mathf.Sign(_velocity.y));
            float distY = Mathf.Abs(_velocity.y * delta);
            if (_rigidbody.Cast(dirY, _castFilter, _hitBuffer, distY) > 0)
                _velocity.y = 0;
        }

        _rigidbody.velocity = _velocity;
    }

    private void PerformDash()
    {
        if (dashTimeLeft == dashDuration)
        {
            SoundManager.Instance.PlayPlayer(PlayerSoundType.Dash);
            _rigidbody.velocity = dashDirection * dashSpeed;
        }

        dashTimeLeft -= Time.fixedDeltaTime;
        if (dashTimeLeft <= 0)
        {
            isDashing = false;
            dashCooldownTimer = dashCooldown;
        }
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
        if (Input.GetKeyDown(KeyCode.W))
        {
            GameAnalytics.Instance.RegisterButtonPress("W");
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            GameAnalytics.Instance.RegisterButtonPress("A");
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            GameAnalytics.Instance.RegisterButtonPress("S");
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            GameAnalytics.Instance.RegisterButtonPress("D");
        }
    }
    public void OnDash(InputAction.CallbackContext context)
    {
        if (_playerHealth.isDead) return;
        if (!canDash || isDashing || dashCooldownTimer > 0f) return;
        if (!context.performed) return;

        dashDirection = _moveInput.sqrMagnitude > 0.1f ? _moveInput.normalized : Vector2.right;
        isDashing = true;
        dashTimeLeft = dashDuration;

    }

    // Обработка ЛКМ. Подбор оружия или атака
    public void OnLeftMouse(InputAction.CallbackContext context)
    {
        if (_shop != null && _shop.isShopping)
            return;

        if (_pauseMenu.isPaused || _playerHealth.isDead || isConversation || _currentScene == "MainMenu")
            return;

        if (context.started)
        {
            GameAnalytics.Instance.RegisterButtonPress("LeftMouseButton");

            if (_weaponManager.GetCurrentWeaponType() == WeaponType.NoWeapon)
            {
                _weaponManager.TryPickUpWeapon();
                Debug.Log("Схвачено оружие");
            }
            else
            {
                WeaponType current = _weaponManager.GetCurrentWeaponType();
                // Запуск автоматической стрельбы для автоматического оружия
                if (current == WeaponType.Uzi || current == WeaponType.Rifle)
                {
                    _weaponManager.StartAutoFire();
                }
                else
                {
                    // Одиночная атака
                    _animator.SetTrigger("Attack");
                    Collider2D[] hitPlayer = Physics2D.OverlapCircleAll(transform.position, 3);
                    foreach (Collider2D col in hitPlayer)
                    {
                        if (col.CompareTag("Enemy"))
                        {
                            Debug.Log("Hit!");
                            StartCoroutine(col.gameObject.GetComponent<Mafia>().InvestigateSound());
                        }
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
        if (_shop != null && _shop.isShopping)
            return;

        if (_pauseMenu.isPaused || _playerHealth.isDead)
            return;

        if (context.performed)
        {
            GameAnalytics.Instance.RegisterButtonPress("RightMouseButton");

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

    public void UnlockDash()
    {
        if (canDash == false) { 
            canDash = true;
        }
    }

    public void ResetPlayer()
    {
        _playerHealth.SetFullHealth();
        _playerHealth.Armor = 0;
        canDash = false;
        _moveInput = Vector2.zero;
        _velocity = Vector2.zero;
        _rigidbody.velocity = Vector2.zero;
        _weaponManager.ResetWeapon();        
    }
}
