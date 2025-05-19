using UnityEngine;

[CreateAssetMenu(menuName = "Weapon/WeaponData", fileName = "NewWeaponData")]
public class WeaponData : ScriptableObject
{
    public WeaponType weaponType;

    [Header("�������� ���������")]
    public float damage;            
    public float attackSpeed;       
    public float attackDelay;       
    public int ammoCapacity;        

    [Header("��������� �������� ���")]
    public float meleeRadius;       

    [Header("��������� �������� ���")]
    public GameObject projectilePrefab; 
    public float projectileSpeed;       
    public int projectileCount = 1;     
    public float spreadAngle;           // ���� �������� ��� ��������� � ��������

    [Header("��������� ����")]
    public Vector3 bulletScale;
    public Color bulletColor;

}
