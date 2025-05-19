using UnityEngine;

[CreateAssetMenu(menuName = "Weapon/WeaponData", fileName = "NewWeaponData")]
public class WeaponData : ScriptableObject
{
    public WeaponType weaponType;

    [Header("Основные параметры")]
    public float damage;            
    public float attackSpeed;       
    public float attackDelay;       
    public int ammoCapacity;        

    [Header("Параметры ближнего боя")]
    public float meleeRadius;       

    [Header("Параметры дальнего боя")]
    public GameObject projectilePrefab; 
    public float projectileSpeed;       
    public int projectileCount = 1;     
    public float spreadAngle;           // Угол разброса для дробовика в градусах

    [Header("Параметры пули")]
    public Vector3 bulletScale;
    public Color bulletColor;

}
