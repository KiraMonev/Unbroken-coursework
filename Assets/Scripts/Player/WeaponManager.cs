using UnityEngine;
using System.Collections.Generic;
using System.Collections;

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

    private WeaponType _currentWeaponType = WeaponType.NoWeapon;
    // ������� ���������� ������ ��� ������, ���� ��� ���������� (ammoCapacity > 0)
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
        Debug.Log($"��������� ������: {_currentWeaponType}");

        // ���� ������ ���������� (ammoCapacity > 0), ������������� ������� ����� ������
        if (_weaponDataDict.TryGetValue(_currentWeaponType, out WeaponData data))
        {
            if (data.ammoCapacity > 0)
            {
                _currentAmmo = data.ammoCapacity;
                Debug.Log($"����������� ��������: {_currentAmmo}");
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
        {
            yield break;
        }

        while (true)
        {
            // ���� ������ ����������, ��������� ������� ��������
            if (data.ammoCapacity > 0)
            {
                if (_currentAmmo <= 0)
                {
                    Debug.Log("��� �������� ��� ������������!");
                    // ����� ������� ���� ������� �������� ��� �����������
                    yield break;
                }
                _currentAmmo--;
                Debug.Log($"�������� ��������: {_currentAmmo}");
            }

            // ����� ��������� �������� ����� ��� ��������, ���� ��� �������������:
            _animator.SetTrigger("Attack");

            // �������� ������� �������� (������ �������� Animation Event) � ����� ���� ����������� �����
            RangeAttackSingle(data);

            // ��� �������� ����� ����������
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
            if (_currentAmmo <= 0)
            {
                Debug.Log("��� ��������!");
                // ����� ����� ������� ���� ������� �������� � �.�.
                return;
            }
        }

        // ����� ������ �����
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
                Debug.LogWarning($"�� ���������� ����� ����� ��� {_currentWeaponType}");
                break;
        }
    }

    // ==== ������� ��� ====
    private void MeleeAttack(WeaponData data)
    {
        // ������: ���� � ������� ������ ������ (��� ����� ������������ FirePoint).
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, data.meleeRadius);
        foreach (Collider2D hit in hits)
        {
            // ����� ���������, ������ �� �� �����
            // hit.CompareTag("Enemy") - ���� ����� �������� ����� "Enemy"
            Debug.Log($"Melee hit: {hit.name}, ������� {data.damage} �����.");
        }
        // ����� ����� ������� ���� ����� ��� ������
    }

    // ==== ��������� ������� (Pistol, Uzi, Rifle) ====
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
            Debug.Log($"������� �� {_currentWeaponType}, ����: {data.damage}");
        }
        else
        {
            Debug.LogError($"Fire point ��� ������ ������� �� �������� ��� {_currentWeaponType}");
        }
    }

    // ==== ������� ��������� (��������� �������� � ���������) ====
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
            Debug.Log($"��������: {count} ��������, ����: {data.damage}");
        }
        else
        {
            Debug.LogError($"Fire point ��� ������ ������� �� �������� ��� {_currentWeaponType}");
        }
    }
}
