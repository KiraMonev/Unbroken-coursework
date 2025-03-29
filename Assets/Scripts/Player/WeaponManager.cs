using UnityEngine;
using System.Collections.Generic;

public class WeaponManager : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private float _pickupRadius = 1f;
    [SerializeField] private float _throwForce = 10f;

    [Header("References")]
    [SerializeField] private List<WeaponPrefabPair> _weaponPrefabs;
    [SerializeField] private List<WeaponAnimatorPair> _weaponAnimatorPairs;
    private PlayerController _playerController;
    private Animator _animator;

    // Базовый контроллер для состояния "Без оружия"
    [SerializeField] private RuntimeAnimatorController _baseAnimatorController;

    private Dictionary<WeaponType, GameObject> _weaponPrefabDict;
    private Dictionary<WeaponType, RuntimeAnimatorController> _weaponAnimatorDict;
    private WeaponType _currentWeaponType = WeaponType.NoWeapon;

    [System.Serializable]
    private class WeaponPrefabPair
    {
        public WeaponType type;
        public GameObject prefab;
    }

    [System.Serializable]
    private class WeaponAnimatorPair
    {
        public WeaponType type;
        public RuntimeAnimatorController controller;
    }

    private void Awake()
    {
        if (_animator == null)
            _animator = GetComponent<Animator>();
        if (_playerController == null)
            _playerController = GetComponent<PlayerController>();

        InitializeWeaponDictionary();
        InitializeWeaponAnimatorDictionary();
    }

    private void InitializeWeaponDictionary()
    {
        _weaponPrefabDict = new Dictionary<WeaponType, GameObject>();
        foreach (var pair in _weaponPrefabs)
        {
            _weaponPrefabDict[pair.type] = pair.prefab;
        }
    }

    private void InitializeWeaponAnimatorDictionary()
    {
        _weaponAnimatorDict = new Dictionary<WeaponType, RuntimeAnimatorController>();
        // Добавляем базовый контроллер для состояния "Без оружия"
        _weaponAnimatorDict[WeaponType.NoWeapon] = _baseAnimatorController;
        foreach (var pair in _weaponAnimatorPairs)
        {
            _weaponAnimatorDict[pair.type] = pair.controller;
        }
    }

    // Подбор оружия в радиусе
    public void TryPickUpWeapon()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, _pickupRadius);
        foreach (Collider2D collider in colliders)
        {
            WeaponPickup pickup = collider.GetComponent<WeaponPickup>();
            if (pickup != null)
            {
                // Подбираем оружие только если сейчас его нет
                if (_currentWeaponType == WeaponType.NoWeapon)
                {
                    PickUpWeapon(pickup);
                }
                return;
            }
        }
    }

    private void PickUpWeapon(WeaponPickup pickup)
    {
        _currentWeaponType = pickup.WeaponType;
        // Меняем аниматор на контроллер, соответствующий подобранному оружию
        ChangeWeaponAnimator(_currentWeaponType);
        Destroy(pickup.gameObject);
    }

    public void DropWeapon()
    {
        if (_currentWeaponType == WeaponType.NoWeapon)
            return;

        if (_weaponPrefabDict.TryGetValue(_currentWeaponType, out GameObject weaponPrefab))
        {
            Vector2 throwDirection = GetThrowDirection();
            Vector2 spawnPosition = (Vector2)transform.position + throwDirection * 1f;

            GameObject droppedWeapon = Instantiate(weaponPrefab, spawnPosition, Quaternion.identity);
            WeaponPickup pickup = droppedWeapon.GetComponent<WeaponPickup>();
            if (pickup != null)
            {
                Vector2 throwVector = throwDirection + Vector2.up * 0.5f;
                pickup.Throw(throwVector.normalized, _throwForce);
            }
        }
        else
        {
            Debug.LogError($"Оружие типа {_currentWeaponType} не найдено в списке префабов.");
        }

        _currentWeaponType = WeaponType.NoWeapon;
        ChangeWeaponAnimator(WeaponType.NoWeapon);
    }

    private Vector2 GetThrowDirection()
    {
        Vector2 playerInput = _playerController.GetMoveInput();
        return playerInput.magnitude > 0.1f ? playerInput.normalized : (Vector2)transform.right;
    }

    private void ChangeWeaponAnimator(WeaponType weaponType)
    {
        if (_weaponAnimatorDict.TryGetValue(weaponType, out RuntimeAnimatorController controller))
        {
            _animator.runtimeAnimatorController = controller;
        }
        else
        {
            Debug.LogError($"Animator controller для оружия {weaponType} не найден.");
        }
    }

    public WeaponType GetCurrentWeaponType() => _currentWeaponType;
}
