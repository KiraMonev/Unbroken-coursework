using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class WeaponManager : MonoBehaviour
{
    [Header("��������� ������")]
    [SerializeField] private float _pickupRadius = 1f;
    [SerializeField] private float _throwForce = 10f;

    [Header("������ �������� ������")]
    [SerializeField] private List<WeaponPrefabPair> _weaponPrefabs;
    [System.Serializable]
    private class WeaponPrefabPair
    {
        public WeaponType type;
        public GameObject prefab;
    }

    [Header("������ ���������� ������")]
    [SerializeField] private List<WeaponAnimatorPair> _weaponAnimatorPairs;
    [System.Serializable]
    private class WeaponAnimatorPair
    {
        public WeaponType type;
        public RuntimeAnimatorController controller;
    }

    [Header("������ ������ (ScriptableObject)")]
    [SerializeField] private List<WeaponData> _weaponDataList;

    // ������� ���������� ��� ��������� "��� ������"
    [SerializeField] private RuntimeAnimatorController _baseAnimatorController;

    [Header("Fire Points")]
    [Tooltip("��������� ������� ���� ������ ���� fire point (�������� ������ �� Player).")]
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

    // ������� ��� �������� �������� ������ �������� ��� ������� ������.
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
        // ��� "��� ������"
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

    // �������� ��� ������� � �������� ������ �������� ������
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

    // ������� ��������� ������ (���������� �� PlayerController ��� ���, ���� ��� ������)
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
                    AchievementManager.instance.Unlock("Example");  // �������� ������ ����������
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
        //Debug.Log($"��������� ������: {_currentWeaponType}");

        // ���� �������
        if (_currentWeaponType == WeaponType.Knife || _currentWeaponType == WeaponType.Katana)
            SoundManager.Instance.PlayPlayer(AudioType.PickupKatanaAndKnife);
        else if (_currentWeaponType == WeaponType.Ballbat)
                SoundManager.Instance.PlayPlayer(AudioType.BallbatAttack);
        else
            SoundManager.Instance.PlayPlayer(AudioType.PickupWeapon);

        // ���� ������ ���������� (ammoCapacity > 0), ������������� ������� ����� ������
        if (_weaponDataDict.TryGetValue(_currentWeaponType, out WeaponData data))
        {
            if (data.ammoCapacity > 0)
            {
                if (!_ammoDict.ContainsKey(_currentWeaponType))
                {
                    // ��� ������ ������� ������������� ������������ ���������� ��������.
                    _ammoDict[_currentWeaponType] = data.ammoCapacity;
                    _bulletsAmoutUI.SetCurrentAmmo(data.ammoCapacity);
                }
                _bulletsAmoutUI.SetTotalAmmo(data.ammoCapacity);
                _bulletsAmoutUI.SetCurrentAmmo(_ammoDict[_currentWeaponType]);
                Debug.Log($"������� ����� ��������: {_ammoDict[_currentWeaponType]}");
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
                // ���� ������
                SoundManager.Instance.PlayPlayer(AudioType.Throw);
                _bulletsAmoutUI.SetTotalAmmo(0);
                _bulletsAmoutUI.SetCurrentAmmo(0);
            }
        }
        else
        {
            Debug.LogError($"������ ���� {_currentWeaponType} �� ������� � ������ ��������.");
        }

        _currentWeaponType = WeaponType.NoWeapon;
        ChangeWeaponAnimator(WeaponType.NoWeapon);
        ActivateFirePoint(WeaponType.NoWeapon);

        Debug.Log("�������� ������");
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
            Debug.LogError($"Animator controller ��� ������ {weaponType} �� ������.");
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

    // ����� ��� ������� �������������� �������� (��� Uzi � Rifle)
    public void StartAutoFire()
    {
        // ���� ��� ��������, �������
        if (_autoFireCoroutine != null)
            return;

        // ��������� �������� ������������
        _autoFireCoroutine = StartCoroutine(AutoFireCoroutine());
    }

    // ����� ��� ��������� �������������� ��������
    public void StopAutoFire()
    {
        if (_autoFireCoroutine != null)
        {
            StopCoroutine(_autoFireCoroutine);
            _autoFireCoroutine = null;
        }
    }

    // ��������, ������� ��������� �������� � �������� ���������, ���� ������ ������������
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

    // === ����� ===
    // ���� ����� ���������� �� Animation Event (� �������� �����).
    public void Attack()
    {
        // ���� ������ �� �������, �������
        if (_currentWeaponType == WeaponType.NoWeapon)
            return;

        // ������ ������ (ScriptableObject) ��� �������� ������
        if (!_weaponDataDict.TryGetValue(_currentWeaponType, out WeaponData data))
        {
            Debug.LogError($"��� WeaponData ��� {_currentWeaponType}");
            return;
        }

        // ��� ����������� ������ ������� ��������� ������� ��������
        if (data.ammoCapacity > 0)
        {
            int ammoCost = (_currentWeaponType == WeaponType.Shotgun) ? data.projectileCount : 1;
            if (CurrentAmmo < ammoCost)
            {
                Debug.Log("��� ��������!");
                SoundManager.Instance.PlayPlayer(AudioType.EmptyAmmo);
                return;
            }
            CurrentAmmo -= ammoCost;
            //Debug.Log($"������� �� {_currentWeaponType}. �������� ��������: {CurrentAmmo}");
        }

        // ����� ������ �����
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
                Debug.LogWarning($"�� ���������� ����� ����� ��� {_currentWeaponType}");
                break;
        }
    }

    // ==== ������� ��� ====
    // ��������� ������� ��� ������
    private void DrawAttackRectangle(Vector2 center, Vector2 size, float angle, Color color, float duration = 0.5f)
    {
        // ��������� �������� ��������
        Vector2 halfSize = size * 0.5f;

        // ��������� ���������� ����� ��������������
        Vector2 topRight = new Vector2(halfSize.x, halfSize.y);
        Vector2 topLeft = new Vector2(-halfSize.x, halfSize.y);
        Vector2 bottomLeft = new Vector2(-halfSize.x, -halfSize.y);
        Vector2 bottomRight = new Vector2(halfSize.x, -halfSize.y);

        // ����������� ���� �� �������� � �������
        float rad = angle * Mathf.Deg2Rad;

        // ������� �������� ��������� ����� ������ (0,0)
        Vector2 Rotate(Vector2 v)
        {
            return new Vector2(
                v.x * Mathf.Cos(rad) - v.y * Mathf.Sin(rad),
                v.x * Mathf.Sin(rad) + v.y * Mathf.Cos(rad)
            );
        }

        // ��������� ������� ���������� �����, ����������� �� � ��������� �����
        topRight = Rotate(topRight) + center;
        topLeft = Rotate(topLeft) + center;
        bottomLeft = Rotate(bottomLeft) + center;
        bottomRight = Rotate(bottomRight) + center;

        // ������������ ����� ����� ������
        Debug.DrawLine(topRight, topLeft, color, duration);
        Debug.DrawLine(topLeft, bottomLeft, color, duration);
        Debug.DrawLine(bottomLeft, bottomRight, color, duration);
        Debug.DrawLine(bottomRight, topRight, color, duration);
    }

    private void MeleeAttack(WeaponData data)
    {
        // ���������� ����������� ����� (����� �� ������)
        Vector2 attackDirection = transform.right;
        // ����� �������� ������ �����: ������� ������� �� ������
        float offsetDistance = data.meleeRadius * 1f;
        Vector2 attackCenter = (Vector2)transform.position + attackDirection * offsetDistance;

        // ���������� ������� ������������� ������� �����
        Vector2 boxSize = new Vector2(data.meleeRadius, data.meleeRadius * 1f);

        // ������������ ���� �������� ������� (� ��������)
        float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
        // ������������ ������������� ��� �������
        DrawAttackRectangle(attackCenter, boxSize, angle, Color.green, 0.5f);

        // �������� ��� ����������, ���������� � ������������� �������
        Collider2D[] hits = Physics2D.OverlapBoxAll(attackCenter, boxSize, angle);

        foreach (Collider2D hit in hits)
        {

            Debug.Log($"Melee hit: {hit.name}, ������� {data.damage} �����.");
                // ����� ����� ������� ����� TakeDamage
                // hit.GetComponent<Enemy>()?.TakeDamage(data.damage);

        }
        // �������� ���� �����
    }


    // ==== ��������� ������� (Pistol) ====
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
            Debug.Log($"������� �� {_currentWeaponType}, ����: {data.damage}. �������� ��������: {CurrentAmmo}");
        }
        else
        {
            Debug.LogError($"Fire point ��� ������ ������� �� �������� ��� {_currentWeaponType}");
        }
    }

    // ==== ������� ��������� (��������� �������� � ���������) ====
    private void RangeAttackShotgun(WeaponData data)
    {

        if (_weaponFirePointDict.TryGetValue(_currentWeaponType, out GameObject fp) && data.projectilePrefab != null)
        {
            int count = data.projectileCount;
            float spread = data.spreadAngle;
            float startAngle = -spread / 2f;
            float step = (count > 1) ? spread / (count - 1) : 0;

            // ������� ����������� �������� (�� fire point)
            Vector2 attackDir = fp.transform.right;
            // ���������������� ����������� (� ��������� XY)
            Vector2 perpendicular = new Vector2(-attackDir.y, attackDir.x);

            // ����������� �������� (��������� ����������������)
            float offsetFactor = 0.1f;

            for (int i = 0; i < count; i++)
            {
                float angleOffset = startAngle + step * i;
                Quaternion rotation = fp.transform.rotation * Quaternion.Euler(0, 0, angleOffset);

                // ��������� �������� ��� ���� ����:
                // ��������� �������� ���, ����� ���� ���������� ����������� �� ���������������� ���.
                float indexOffset = ((float)i - (count - 1) / 2f) * offsetFactor;
                Vector2 spawnPos = (Vector2)fp.transform.position + perpendicular * indexOffset;


                GameObject proj = Instantiate(data.projectilePrefab, spawnPos, rotation);
                Bullet bullet = proj.GetComponent<Bullet>();
                if (bullet != null)
                {
                    bullet.SetParameters(data.damage, data.projectileSpeed, data.bulletScale, data.bulletColor);
                }
            }
            Debug.Log($"��������: {count} ��������, ����: {data.damage}. �������� ��������: {CurrentAmmo}");
        }
        else
        {
            Debug.LogError($"Fire point ��� ������ ������� �� �������� ��� {_currentWeaponType}");
        }
    }

    // �����, ���������� ��� ������� ����������� (AmmoPickup).
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
                Debug.Log($"���������� ��� {_currentWeaponType} ��������� �� {data.ammoCapacity}");
            }
        }
    }

}
