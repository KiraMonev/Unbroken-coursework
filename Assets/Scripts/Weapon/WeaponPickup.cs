using UnityEngine;
using System.Collections;

public class WeaponPickup : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private WeaponType _weaponType;
    public WeaponType WeaponType => _weaponType;

    [Header("Weapon Data")]
    [SerializeField] private WeaponData _weaponData;
    [Header("Current Ammo")]
    [SerializeField] private int _currentAmmo;
    public int CurrentAmmo
    {
        get => _currentAmmo;
        set => _currentAmmo = Mathf.Clamp(value, 0, _weaponData != null ? _weaponData.ammoCapacity : value);
    }

    [Header("Throw Settings")]
    [SerializeField] private Rigidbody2D _rigidBody;
    [SerializeField] private Collider2D _collider;
    [Tooltip("Layers considered 'solid' for stopping the thrown weapon")]
    [SerializeField] private LayerMask _collidableLayerMask;
    [Tooltip("Delay before re-enabling collision with player")]
    [SerializeField] private float _enableTriggerDelay = 0.5f;

    // Для ручной детекции проскальзывания сквозь объекты
    private Vector2 _prevPosition;

    private void Awake()
    {
        if (_rigidBody == null)
            _rigidBody = GetComponent<Rigidbody2D>();
        if (_collider == null)
            _collider = GetComponent<Collider2D>();

        if (_weaponData != null)
            _currentAmmo = _weaponData.ammoCapacity;
        else
            Debug.LogWarning("WeaponPickup: _weaponData не назначена");
    }

    private void Start()
    {
        // Стартовая позиция
        _prevPosition = _rigidBody.position;

        // Отключаем коллизию с игроком сразу после броска
        var player = PlayerController.Instance;
        if (player != null)
        {
            var playerCol = player.GetComponent<Collider2D>();
            if (playerCol != null)
            {
                Physics2D.IgnoreCollision(_collider, playerCol, true);
                StartCoroutine(ReenableCollisionAfterDelay(playerCol));
            }
        }
    }

    private IEnumerator ReenableCollisionAfterDelay(Collider2D playerCollider)
    {
        yield return new WaitForSeconds(_enableTriggerDelay);
        if (_collider != null && playerCollider != null)
            Physics2D.IgnoreCollision(_collider, playerCollider, false);
    }

    private void FixedUpdate()
    {
        // Если летим — проверяем, не прошли ли сквозь стену/предмет
        if (_rigidBody.velocity.sqrMagnitude > 0.01f)
        {
            Vector2 currentPos = _rigidBody.position;
            RaycastHit2D hit = Physics2D.Linecast(
                _prevPosition,
                currentPos,
                _collidableLayerMask
            );
            if (hit.collider != null)
            {
                _rigidBody.position = hit.point;
                StopAndMakeTrigger();
            }
            _prevPosition = currentPos;
        }
    }


    //Вызывается из WeaponManager
    public void Throw(Vector2 direction, float force)
    {
        if (_rigidBody == null || _collider == null) return;

        _collider.isTrigger = false;
        _rigidBody.isKinematic = false;
        _rigidBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _rigidBody.velocity = direction * force;

        StartCoroutine(EnableTriggerAfterDelay());
    }

    private IEnumerator EnableTriggerAfterDelay()
    {
        yield return new WaitForSeconds(_enableTriggerDelay);
        StopAndMakeTrigger();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        StopAndMakeTrigger();
    }

    private void StopAndMakeTrigger()
    {
        if (_rigidBody != null)
        {
            _rigidBody.velocity = Vector2.zero;
            _rigidBody.angularVelocity = 0f;
        }
        if (_collider != null)
            _collider.isTrigger = true;
    }
}
