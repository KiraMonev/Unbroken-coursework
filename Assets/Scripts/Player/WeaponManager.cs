using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

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

    // Словарь для хранения текущего запаса патронов для каждого оружия.
    private Dictionary<WeaponType, int> _ammoDict = new Dictionary<WeaponType, int>();

    private WeaponType _currentWeaponType = WeaponType.NoWeapon;

    private Coroutine _autoFireCoroutine;

    private PlayerController _playerController;
    private Animator _animator;
    private PlayerHealth _playerHealth;
    private UIBulletsAmount _bulletsAmoutUI;
    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _animator = GetComponent<Animator>();
        _playerHealth = GetComponent<PlayerHealth>();
        _bulletsAmoutUI = FindObjectOfType<UIBulletsAmount>();

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

    // Свойство для доступа к текущему запасу патронов оружия
    private int CurrentAmmo
    {
        get
        {
            if (_ammoDict.ContainsKey(_currentWeaponType))
            {
                _bulletsAmoutUI.SetCurrentAmmo(_ammoDict[_currentWeaponType]);
                return _ammoDict[_currentWeaponType];
            }
            _bulletsAmoutUI.SetCurrentAmmo(0);
            return 0;
        }
        set
        {
            if (_ammoDict.ContainsKey(_currentWeaponType))
                _ammoDict[_currentWeaponType] = value;
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
                    AchievementManager.instance.Unlock("Example");  // Проверка работы достижений
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
        //Debug.Log($"Подобрали оружие: {_currentWeaponType}");

        // Звук подбора
        if (_currentWeaponType == WeaponType.Knife || _currentWeaponType == WeaponType.Katana)
            SoundManager.Instance.PlayPlayer(AudioType.PickupKatanaAndKnife);
        else if (_currentWeaponType == WeaponType.Ballbat)
                SoundManager.Instance.PlayPlayer(AudioType.BallbatAttack);
        else
            SoundManager.Instance.PlayPlayer(AudioType.PickupWeapon);

        // Если оружие стрелковое (ammoCapacity > 0), устанавливаем текущий запас патрон
        if (_weaponDataDict.TryGetValue(_currentWeaponType, out WeaponData data))
        {
            if (data.ammoCapacity > 0)
            {
                if (!_ammoDict.ContainsKey(_currentWeaponType))
                {
                    // При первом подборе устанавливаем максимальное количество патронов.
                    _ammoDict[_currentWeaponType] = data.ammoCapacity;
                    _bulletsAmoutUI.SetCurrentAmmo(data.ammoCapacity);
                }
                _bulletsAmoutUI.SetTotalAmmo(data.ammoCapacity);
                _bulletsAmoutUI.SetCurrentAmmo(_ammoDict[_currentWeaponType]);
                Debug.Log($"Текущий запас патронов: {_ammoDict[_currentWeaponType]}");
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
                // Звук броска
                SoundManager.Instance.PlayPlayer(AudioType.Throw);
                _bulletsAmoutUI.SetTotalAmmo(0);
                _bulletsAmoutUI.SetCurrentAmmo(0);
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
            yield break;

        while (true)
        {
            if (_playerHealth.isDead)
            {
                StopAutoFire();
                yield break;
            }
            _animator.SetTrigger("Attack");
            Attack();
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
            int ammoCost = (_currentWeaponType == WeaponType.Shotgun) ? data.projectileCount : 1;
            if (CurrentAmmo < ammoCost)
            {
                Debug.Log("Нет патронов!");
                SoundManager.Instance.PlayPlayer(AudioType.EmptyAmmo);
                return;
            }
            CurrentAmmo -= ammoCost;
            //Debug.Log($"Выстрел из {_currentWeaponType}. Осталось патронов: {CurrentAmmo}");
        }

        // Выбор логики атаки
        switch (_currentWeaponType)
        {
            case WeaponType.Ballbat:
                SoundManager.Instance.PlayPlayer(AudioType.BallbatAttack);
                MeleeAttack(data);
                break;

            case WeaponType.Knife:
            case WeaponType.Katana:
                SoundManager.Instance.PlayPlayer(AudioType.KatanaAndKnife);
                MeleeAttack(data);
                break;

            case WeaponType.Pistol:
                SoundManager.Instance.PlayPlayer(AudioType.PistolShoot);
                RangeAttackSingle(data);
                break;

            case WeaponType.Uzi:
                SoundManager.Instance.PlayPlayer(AudioType.UziShoot);
                RangeAttackSingle(data);
                break;

            case WeaponType.Rifle:
                SoundManager.Instance.PlayPlayer(AudioType.RifleShoot);
                RangeAttackSingle(data);
                break;

            case WeaponType.Shotgun:
                SoundManager.Instance.PlayPlayer(AudioType.ShotgunShoot);
                RangeAttackShotgun(data);
                break;

            default:
                Debug.LogWarning($"Не реализован метод атаки для {_currentWeaponType}");
                break;
        }
    }

    // ==== Ближний бой ====
    // Временная функция для дебага
    private void DrawAttackRectangle(Vector2 center, Vector2 size, float angle, Color color, float duration = 0.5f)
    {
        // Вычисляем половины размеров
        Vector2 halfSize = size * 0.5f;

        // Локальные координаты углов прямоугольника
        Vector2 topRight = new Vector2(halfSize.x, halfSize.y);
        Vector2 topLeft = new Vector2(-halfSize.x, halfSize.y);
        Vector2 bottomLeft = new Vector2(-halfSize.x, -halfSize.y);
        Vector2 bottomRight = new Vector2(halfSize.x, -halfSize.y);

        // Преобразуем угол из градусов в радианы
        float rad = angle * Mathf.Deg2Rad;

        // Функция поворота локальной точки вокруг (0,0)
        Vector2 Rotate(Vector2 v)
        {
            return new Vector2(
                v.x * Mathf.Cos(rad) - v.y * Mathf.Sin(rad),
                v.x * Mathf.Sin(rad) + v.y * Mathf.Cos(rad)
            );
        }

        // Вычисляем мировые координаты углов, поворачивая их и прибавляя центр
        topRight = Rotate(topRight) + center;
        topLeft = Rotate(topLeft) + center;
        bottomLeft = Rotate(bottomLeft) + center;
        bottomRight = Rotate(bottomRight) + center;

        // Отрисовываем линии между углами
        Debug.DrawLine(topRight, topLeft, color, duration);
        Debug.DrawLine(topLeft, bottomLeft, color, duration);
        Debug.DrawLine(bottomLeft, bottomRight, color, duration);
        Debug.DrawLine(bottomRight, topRight, color, duration);
    }

    private void MeleeAttack(WeaponData data)
    {
        // Определяем направление атаки (вперёд от игрока)
        Vector2 attackDirection = transform.right;
        // Задаём смещение центра атаки: немного спереди от игрока
        float offsetDistance = data.meleeRadius * 1f;
        Vector2 attackCenter = (Vector2)transform.position + attackDirection * offsetDistance;

        // Определяем размеры прямоугольной области атаки
        Vector2 boxSize = new Vector2(data.meleeRadius, data.meleeRadius * 1f);

        // Рассчитываем угол поворота области (в градусах)
        float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
        // Отрисовываем прямоугольник для отладки
        DrawAttackRectangle(attackCenter, boxSize, angle, Color.green, 0.5f);

        // Получаем все коллайдеры, попадающие в прямоугольную область
        Collider2D[] hits = Physics2D.OverlapBoxAll(attackCenter, boxSize, angle);

        foreach (Collider2D hit in hits)
        {

            Debug.Log($"Melee hit: {hit.name}, наносим {data.damage} урона.");
                // Здесь можно вызвать метод TakeDamage
                // hit.GetComponent<Enemy>()?.TakeDamage(data.damage);

        }
        // добавить звук удара
    }


    // ==== Одиночный выстрел (Pistol) ====
    private void RangeAttackSingle(WeaponData data)
    {

        if (_weaponFirePointDict.TryGetValue(_currentWeaponType, out GameObject fp) && data.projectilePrefab != null)
        {
            GameObject proj = Instantiate(data.projectilePrefab, fp.transform.position, fp.transform.rotation);
            Bullet bullet = proj.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.SetParameters(data.damage, data.projectileSpeed, data.bulletScale, data.bulletColor);
            }
            Debug.Log($"Выстрел из {_currentWeaponType}, урон: {data.damage}. Осталось патронов: {CurrentAmmo}");
        }
        else
        {
            Debug.LogError($"Fire point или префаб снаряда не назначен для {_currentWeaponType}");
        }
    }

    // ==== Выстрел дробовика (несколько снарядов с разбросом) ====
    private void RangeAttackShotgun(WeaponData data)
    {

        if (_weaponFirePointDict.TryGetValue(_currentWeaponType, out GameObject fp) && data.projectilePrefab != null)
        {
            int count = data.projectileCount;
            float spread = data.spreadAngle;
            float startAngle = -spread / 2f;
            float step = (count > 1) ? spread / (count - 1) : 0;

            // Базовое направление выстрела (от fire point)
            Vector2 attackDir = fp.transform.right;
            // Перпендикулярное направление (в плоскости XY)
            Vector2 perpendicular = new Vector2(-attackDir.y, attackDir.x);

            // Коэффициент смещения (подберите экспериментально)
            float offsetFactor = 0.1f;

            for (int i = 0; i < count; i++)
            {
                float angleOffset = startAngle + step * i;
                Quaternion rotation = fp.transform.rotation * Quaternion.Euler(0, 0, angleOffset);

                // Вычисляем смещение для этой пули:
                // Вычисляем смещение так, чтобы пули равномерно расходились по перпендикулярной оси.
                float indexOffset = ((float)i - (count - 1) / 2f) * offsetFactor;
                Vector2 spawnPos = (Vector2)fp.transform.position + perpendicular * indexOffset;


                GameObject proj = Instantiate(data.projectilePrefab, spawnPos, rotation);
                Bullet bullet = proj.GetComponent<Bullet>();
                if (bullet != null)
                {
                    bullet.SetParameters(data.damage, data.projectileSpeed, data.bulletScale, data.bulletColor);
                }
            }
            Debug.Log($"Дробовик: {count} снарядов, урон: {data.damage}. Осталось патронов: {CurrentAmmo}");
        }
        else
        {
            Debug.LogError($"Fire point или префаб снаряда не назначен для {_currentWeaponType}");
        }
    }

    // Метод, вызываемый при подборе боеприпасов (AmmoPickup).
    public void RefillAmmo()
    {
        if (_currentWeaponType == WeaponType.NoWeapon)
            return;
        if (_weaponDataDict.TryGetValue(_currentWeaponType, out WeaponData data))
        {
            if (data.ammoCapacity > 0)
            {
                _ammoDict[_currentWeaponType] = data.ammoCapacity;
                _bulletsAmoutUI.SetCurrentAmmo(_ammoDict[_currentWeaponType]);
                Debug.Log($"Боеприпасы для {_currentWeaponType} пополнены до {data.ammoCapacity}");
            }
        }
    }

}
