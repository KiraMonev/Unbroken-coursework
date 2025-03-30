using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class WeaponManager : MonoBehaviour
{
    [Header("Настройки оружия")]
    [SerializeField] private float _pickupRadius = 1f;
    [SerializeField] private float _throwForce = 10f;

    [Header("Список префабов оружия")]
    [SerializeField] private List<WeaponPrefabPair> _weaponPrefabs;
    [System.Serializable]
    private class WeaponPrefabPair
    {
        public WeaponType type;
        public GameObject prefab;
    }

    [Header("Список аниматоров оружия")]
    [SerializeField] private List<WeaponAnimatorPair> _weaponAnimatorPairs;
    [System.Serializable]
    private class WeaponAnimatorPair
    {
        public WeaponType type;
        public RuntimeAnimatorController controller;
    }

    [Header("Данные оружия (ScriptableObject)")]
    [SerializeField] private List<WeaponData> _weaponDataList;

    // Базовый контроллер для состояния "Без оружия"
    [SerializeField] private RuntimeAnimatorController _baseAnimatorController;

    [Header("Fire Points")]
    [Tooltip("Сопоставь каждому типу оружия свой fire point (дочерний объект на Player).")]
    [SerializeField] private List<WeaponFirePoint> _weaponFirePoints;
    [System.Serializable]
    public class WeaponFirePoint
    {
        public WeaponType weaponType;
        public GameObject firePoint;
    }

    private Dictionary<WeaponType, GameObject> _weaponPrefabDict;
    private Dictionary<WeaponType, RuntimeAnimatorController> _weaponAnimatorDict;
    private Dictionary<WeaponType, WeaponData> _weaponDataDict;
    private Dictionary<WeaponType, GameObject> _weaponFirePointDict;

    private WeaponType _currentWeaponType = WeaponType.NoWeapon;
    // Текущее количество патрон для оружия, если оно стрелковое (ammoCapacity > 0)
    private int _currentAmmo = 0;
    private Coroutine _autoFireCoroutine;

    private PlayerController _playerController;
    private Animator _animator;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _animator = GetComponent<Animator>();

        InitializeWeaponDictionary();
        InitializeWeaponAnimatorDictionary();
        InitializeWeaponDataDictionary();
        InitializeFirePointDictionary();
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
        // Для "без оружия"
        _weaponAnimatorDict[WeaponType.NoWeapon] = _baseAnimatorController;

        foreach (var pair in _weaponAnimatorPairs)
        {
            _weaponAnimatorDict[pair.type] = pair.controller;
        }
    }

    private void InitializeWeaponDataDictionary()
    {
        _weaponDataDict = new Dictionary<WeaponType, WeaponData>();
        foreach (var data in _weaponDataList)
        {
            _weaponDataDict[data.weaponType] = data;
        }
    }

    private void InitializeFirePointDictionary()
    {
        _weaponFirePointDict = new Dictionary<WeaponType, GameObject>();
        foreach (var pair in _weaponFirePoints)
        {
            _weaponFirePointDict[pair.weaponType] = pair.firePoint;
        }
    }

    // Попытка подобрать оружие (вызывается из PlayerController при ЛКМ, если нет оружия)
    public void TryPickUpWeapon()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, _pickupRadius);
        foreach (Collider2D collider in colliders)
        {
            WeaponPickup pickup = collider.GetComponent<WeaponPickup>();
            if (pickup != null)
            {
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
        ChangeWeaponAnimator(_currentWeaponType);
        ActivateFirePoint(_currentWeaponType);
        Destroy(pickup.gameObject);
        Debug.Log($"Подобрали оружие: {_currentWeaponType}");

        // Если оружие стрелковое (ammoCapacity > 0), устанавливаем текущий запас патрон
        if (_weaponDataDict.TryGetValue(_currentWeaponType, out WeaponData data))
        {
            if (data.ammoCapacity > 0)
            {
                _currentAmmo = data.ammoCapacity;
                Debug.Log($"Установлено патронов: {_currentAmmo}");
            }
        }
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
        ActivateFirePoint(WeaponType.NoWeapon);

        Debug.Log("Сбросили оружие");
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

    private void ActivateFirePoint(WeaponType activeType)
    {
        foreach (var pair in _weaponFirePoints)
        {
            if (pair.firePoint != null)
                pair.firePoint.SetActive(pair.weaponType == activeType);
        }
    }

    public WeaponType GetCurrentWeaponType() => _currentWeaponType;

    // Метод для запуска автоматической стрельбы (для Uzi и Rifle)
    public void StartAutoFire()
    {
        // Если уже запущено, выходим
        if (_autoFireCoroutine != null)
            return;

        // Запускаем корутину автострельбы
        _autoFireCoroutine = StartCoroutine(AutoFireCoroutine());
    }

    // Метод для остановки автоматической стрельбы
    public void StopAutoFire()
    {
        if (_autoFireCoroutine != null)
        {
            StopCoroutine(_autoFireCoroutine);
            _autoFireCoroutine = null;
        }
    }

    // Корутина, которая запускает выстрелы с заданной задержкой, пока кнопка удерживается
    private IEnumerator AutoFireCoroutine()
    {
        if (!_weaponDataDict.TryGetValue(_currentWeaponType, out WeaponData data))
        {
            yield break;
        }

        while (true)
        {
            // Если оружие стрелковое, проверяем наличие патронов
            if (data.ammoCapacity > 0)
            {
                if (_currentAmmo <= 0)
                {
                    Debug.Log("Нет патронов для автострельбы!");
                    // Можно вызвать звук пустого магазина или перезарядку
                    yield break;
                }
                _currentAmmo--;
                Debug.Log($"Осталось патронов: {_currentAmmo}");
            }

            // Можно запустить анимацию атаки для автоогня, если она предусмотрена:
            _animator.SetTrigger("Attack");

            // Вызываем выстрел напрямую (вместо ожидания Animation Event) – чтобы пуля создавалась сразу
            RangeAttackSingle(data);

            // Ждём задержку между выстрелами
            yield return new WaitForSeconds(data.attackDelay);
        }
    }

    // === ВАЖНО ===
    // Этот метод вызывается из Animation Event (в анимации атаки).
    public void Attack()
    {
        // Если оружие не выбрано, выходим
        if (_currentWeaponType == WeaponType.NoWeapon)
            return;

        // Достаём данные (ScriptableObject) для текущего оружия
        if (!_weaponDataDict.TryGetValue(_currentWeaponType, out WeaponData data))
        {
            Debug.LogError($"Нет WeaponData для {_currentWeaponType}");
            return;
        }

        // Для стрелкового оружия сначала проверяем наличие патронов
        if (data.ammoCapacity > 0)
        {
            if (_currentAmmo <= 0)
            {
                Debug.Log("Нет патронов!");
                // Здесь можно вызвать звук пустого магазина и т.п.
                return;
            }
        }

        // Выбор логики атаки
        switch (_currentWeaponType)
        {
            case WeaponType.Knife:
            case WeaponType.Katana:
            case WeaponType.Ballbat:
                MeleeAttack(data);
                break;

            case WeaponType.Pistol:
                RangeAttackSingle(data);
                break;
            //case WeaponType.Uzi:
            //case WeaponType.Rifle:
                //RangeAttackSingle(data);
                //break;

            case WeaponType.Shotgun:
                RangeAttackShotgun(data);
                break;

            default:
                Debug.LogWarning($"Не реализован метод атаки для {_currentWeaponType}");
                break;
        }
    }

    // ==== Ближний бой ====
    private void MeleeAttack(WeaponData data)
    {
        // Пример: бьём в радиусе вокруг игрока (или можно использовать FirePoint).
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, data.meleeRadius);
        foreach (Collider2D hit in hits)
        {
            // Здесь проверяем, попали ли во врага
            // hit.CompareTag("Enemy") - если враги помечены тегом "Enemy"
            Debug.Log($"Melee hit: {hit.name}, наносим {data.damage} урона.");
        }
        // Здесь можно вызвать звук удара или эффект
    }

    // ==== Одиночный выстрел (Pistol, Uzi, Rifle) ====
    private void RangeAttackSingle(WeaponData data)
    {
        _currentAmmo--;

        if (_weaponFirePointDict.TryGetValue(_currentWeaponType, out GameObject fp) && data.projectilePrefab != null)
        {
            GameObject proj = Instantiate(data.projectilePrefab, fp.transform.position, fp.transform.rotation);
            Bullet bullet = proj.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.SetParameters(data.damage, data.projectileSpeed);
            }
            Debug.Log($"Выстрел из {_currentWeaponType}, урон: {data.damage}");
        }
        else
        {
            Debug.LogError($"Fire point или префаб снаряда не назначен для {_currentWeaponType}");
        }
    }

    // ==== Выстрел дробовика (несколько снарядов с разбросом) ====
    private void RangeAttackShotgun(WeaponData data)
    {
        _currentAmmo -= data.projectileCount;

        if (_weaponFirePointDict.TryGetValue(_currentWeaponType, out GameObject fp) && data.projectilePrefab != null)
        {
            int count = data.projectileCount;
            float spread = data.spreadAngle;
            float startAngle = -spread / 2f;
            float step = (count > 1) ? spread / (count - 1) : 0;

            for (int i = 0; i < count; i++)
            {
                float angleOffset = startAngle + step * i;
                Quaternion rotation = fp.transform.rotation * Quaternion.Euler(0, 0, angleOffset);
                GameObject proj = Instantiate(data.projectilePrefab, fp.transform.position, rotation);
                Bullet bullet = proj.GetComponent<Bullet>();
                if (bullet != null)
                {
                    bullet.SetParameters(data.damage, data.projectileSpeed);
                }
            }
            Debug.Log($"Дробовик: {count} снарядов, урон: {data.damage}");
        }
        else
        {
            Debug.LogError($"Fire point или префаб снаряда не назначен для {_currentWeaponType}");
        }
    }
}
