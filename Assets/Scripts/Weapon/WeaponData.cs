using UnityEngine;

[CreateAssetMenu(menuName = "Weapon/WeaponData", fileName = "NewWeaponData")]
public class WeaponData : ScriptableObject
{
    public WeaponType weaponType;

    [Header("�������� ���������")]
    public float damage;            // ���� ������
    public float attackSpeed;       // �������� ����� (����� = �������� ��������, �� ����� �����)
    public float attackDelay;       // �������� ����� �������/����������
    public int ammoCapacity;        // ����� ������ (0, ���� melee)

    [Header("��������� �������� ���")]
    public float meleeRadius;       // ������ ��������� (OverlapCircle)

    [Header("��������� �������� ���")]
    public GameObject projectilePrefab; // ������ �������
    public float projectileSpeed;       // �������� �������
    public int projectileCount = 1;     // ���������� �������� (�������� = 3+)
    public float spreadAngle;           // ���� �������� ��� ��������� (� ��������)
}
