using UnityEngine;
using System.Collections;

public class WeaponPickup : MonoBehaviour
{
    [SerializeField] private WeaponType _weaponType;
    public WeaponType WeaponType => _weaponType;

    [Header("Данные оружия")]
    [SerializeField] private WeaponData _weaponData;
    [Header("Текущий боезапас")]
    [SerializeField] private int _currentAmmo;
    public int CurrentAmmo
    {
        get => _currentAmmo;
        set
        {
            _currentAmmo = Mathf.Clamp(value, 0, _weaponData != null ? _weaponData.ammoCapacity : value);
        }
    }

    [Header("Настройки броска")]
    [SerializeField] private Rigidbody2D _rigidBody;
    [SerializeField] private Collider2D _collider;
    [SerializeField] private float _enableTriggerDelay = 0.5f;

    private void Awake()
    {
        if (_rigidBody == null)
            _rigidBody = GetComponent<Rigidbody2D>();
        if (_collider == null)
            _collider = GetComponent<Collider2D>();
        if (_weaponData != null)
            _currentAmmo = _weaponData.ammoCapacity;
        else
            Debug.Log("The weapon is missing _weaponData.");
    }

    private void Start()
    {
        // Игнорируем коллизии между оружием и игроком на короткое время
        Collider2D playerCollider = Object.FindFirstObjectByType<PlayerController>().GetComponent<Collider2D>();
        if (playerCollider != null)
        {
            Physics2D.IgnoreCollision(_collider, playerCollider, true);
            StartCoroutine(ReenableCollisionAfterDelay(playerCollider));
        }
    }

    private IEnumerator ReenableCollisionAfterDelay(Collider2D playerCollider)
    {
        yield return new WaitForSeconds(0.5f);
        Physics2D.IgnoreCollision(_collider, playerCollider, false);
    }

    public void Throw(Vector2 direction, float force)
    {
        if (_rigidBody != null)
        {
            _collider.isTrigger = false;
            _rigidBody.isKinematic = false;
            _rigidBody.velocity = direction * force;
            StartCoroutine(EnableTriggerAfterDelay());
        }
    }

    private IEnumerator EnableTriggerAfterDelay()
    {
        yield return new WaitForSeconds(_enableTriggerDelay);
        _collider.isTrigger = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Останавливаем оружие при столкновении
        if (_rigidBody != null)
        {
            _rigidBody.velocity = Vector2.zero;
            _rigidBody.angularVelocity = 0f;
        }
        _collider.isTrigger = true;
    }
}