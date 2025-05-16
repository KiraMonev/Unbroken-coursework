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

    // ������� ��������� ��� � ��������
    private WeaponType _currentWeaponType = WeaponType.NoWeapon;
    private int _currentAmmo;
    private int _totalAmmo;
    [SerializeField] private bool _doubleDamageUnlocked = false;

    private Coroutine _autoFireCoroutine;

    private PlayerController _playerController;
    private Animator _animator;
    private PlayerHealth _playerHealth;
    private UIBulletsAmount _uiAmmo;

    private ManagerLevel1 _mgr;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _animator = GetComponent<Animator>();
        _playerHealth = GetComponent<PlayerHealth>();
        _uiAmmo = FindObjectOfType<UIBulletsAmount>();
        try
        {
            _mgr = FindObjectOfType<ManagerLevel1>();
        }
        catch
        {
            _mgr = null;
        }

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
            _uiAmmo.SetCurrentAmmo(_currentAmmo);
            return _currentAmmo;
        }
        set
        {
            _currentAmmo = Mathf.Clamp(value, 0, _totalAmmo);
            _uiAmmo.SetCurrentAmmo(_currentAmmo);
        }
    }

    // ������� ��������� ������ (���������� �� PlayerController ��� ���, ���� ��� ������)
    public void TryPickUpWeapon()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, _pickupRadius);
        WeaponPickup nearest = null;
        float bestDistSqr = float.MaxValue;

        foreach (var col in colliders)
        {
            var pickup = col.GetComponent<WeaponPickup>();
            if (pickup == null) continue;

            float distSqr = (pickup.transform.position - transform.position).sqrMagnitude;
            if (distSqr < bestDistSqr)
            {
                bestDistSqr = distSqr;
                nearest = pickup;
            }
        }

        if (nearest != null && _currentWeaponType == WeaponType.NoWeapon)
        {
            PickUpWeapon(nearest);
            AchievementManager.instance.Unlock("firstWeapon");
        }
    }


    private void PickUpWeapon(WeaponPickup pickup)
    {
        var type = pickup.WeaponType;
        var data = _weaponDataDict[type];

        _currentWeaponType = type;
        ChangeWeaponAnimator(type);
        ActivateFirePoint(type);

        // �������� �������� �� ������
        if (data.ammoCapacity > 0)
        {
            _currentAmmo = pickup.CurrentAmmo;
            _totalAmmo = data.ammoCapacity;
            _uiAmmo.SetTotalAmmo(_totalAmmo);
            _uiAmmo.SetCurrentAmmo(_currentAmmo);
        }

        Destroy(pickup.gameObject);

        // ����
        if (type == WeaponType.Knife || type == WeaponType.Katana)
            SoundManager.Instance.PlayWeapon(WeaponSoundType.PickupKatanaAndKnife);
        else if (type == WeaponType.Ballbat)
            SoundManager.Instance.PlayWeapon(WeaponSoundType.BallbatAttack);
        else
            SoundManager.Instance.PlayWeapon(WeaponSoundType.PickupWeapon);
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
                pickup.CurrentAmmo = _currentAmmo;
                pickup.Throw(throwVector.normalized, _throwForce);
                // ���� ������
                SoundManager.Instance.PlayWeapon(WeaponSoundType.Throw);
                _uiAmmo.SetTotalAmmo(0);
                _uiAmmo.SetCurrentAmmo(0);
            }
        }
        else
        {
            Debug.LogError($"������ ���� {_currentWeaponType} �� ������� � ������ ��������.");
        }
        _currentAmmo = 0;
        _totalAmmo = 0;
        _currentWeaponType = WeaponType.NoWeapon;
        ChangeWeaponAnimator(WeaponType.NoWeapon);
        ActivateFirePoint(WeaponType.NoWeapon);

        Debug.Log("�������� ������");
    }

    private Vector2 GetThrowDirection()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = (mouseWorld - transform.position);
        if (dir.sqrMagnitude < 0.01f)
            return transform.right;     //���� ������� ������
        return dir.normalized;
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
                SoundManager.Instance.PlayWeapon(WeaponSoundType.EmptyAmmo);
                return;
            }
            CurrentAmmo -= ammoCost;
            //Debug.Log($"������� �� {_currentWeaponType}. �������� ��������: {CurrentAmmo}");
        }

        // ����� ������ �����
        switch (_currentWeaponType)
        {
            case WeaponType.Ballbat:
                SoundManager.Instance.PlayWeapon(WeaponSoundType.BallbatAttack);
                MeleeAttack(data);
                break;

            case WeaponType.Knife:
            case WeaponType.Katana:
                SoundManager.Instance.PlayWeapon(WeaponSoundType.KatanaAndKnife);
                MeleeAttack(data);
                break;

            case WeaponType.Pistol:
                SoundManager.Instance.PlayWeapon(WeaponSoundType.PistolShoot);
                RangeAttackSingle(data);
                _mgr.RegisterPistolShot();
                break;

            case WeaponType.Uzi:
                SoundManager.Instance.PlayWeapon(WeaponSoundType.UziShoot);
                RangeAttackSingle(data);
                break;

            case WeaponType.Rifle:
                SoundManager.Instance.PlayWeapon(WeaponSoundType.RifleShoot);
                RangeAttackSingle(data);
                break;

            case WeaponType.Shotgun:
                SoundManager.Instance.PlayWeapon(WeaponSoundType.ShotgunShoot);
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
            if (!hit.CompareTag("Enemy"))
                continue;
            float multiplier = _doubleDamageUnlocked ? 2f : 1f;
            int dmg = Mathf.RoundToInt(data.damage * multiplier);
            Debug.Log($"Melee hit: {hit.name}, ������� {data.damage} �����.");
            var mafia = hit.GetComponent<Mafia>();
            if (mafia != null)
            {
                mafia.TakeDamage(dmg);
            }
        }
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
                float multiplier = _doubleDamageUnlocked ? 2f : 1f;
                bullet.SetParameters(data.damage * multiplier, data.projectileSpeed, data.bulletScale, data.bulletColor);
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
            float multiplier = _doubleDamageUnlocked ? 2f : 1f;
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
                    bullet.SetParameters(data.damage * multiplier, data.projectileSpeed, data.bulletScale, data.bulletColor);
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
            // ������ ��� ����������
            if (data.ammoCapacity > 0)
            {
                CurrentAmmo = data.ammoCapacity;
                _totalAmmo = data.ammoCapacity;

                Debug.Log($"�������� ��� {_currentWeaponType} �������� �� {_currentAmmo}.");
            }
        }
        else
        {
            Debug.LogError($"��� ������ WeaponData ��� {_currentWeaponType} ��� ������� ��������� �������.");
        }
    }

    //Activate damage X2 in the SHOP
    public void UnlockDoubleDamage()
    {
        if (_doubleDamageUnlocked == false)
        {
            _doubleDamageUnlocked = true;
            Debug.Log("Activate damage X2");
        }
    }

}
