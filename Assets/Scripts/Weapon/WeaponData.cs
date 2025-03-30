using UnityEngine;

[CreateAssetMenu(menuName = "Weapon/WeaponData", fileName = "NewWeaponData")]
public class WeaponData : ScriptableObject
{
    public WeaponType weaponType;

    [Header("Основные параметры")]
    public float damage;            // Урон оружия
    public float attackSpeed;       // Скорость атаки (часто = скорость анимации, но можно иначе)
    public float attackDelay;       // Задержка между ударами/выстрелами
    public int ammoCapacity;        // Лимит патрон (0, если melee)

    [Header("Параметры ближнего боя")]
    public float meleeRadius;       // Радиус поражения (OverlapCircle)

    [Header("Параметры дальнего боя")]
    public GameObject projectilePrefab; // Префаб снаряда
    public float projectileSpeed;       // Скорость снаряда
    public int projectileCount = 1;     // Количество снарядов (дробовик = 3+)
    public float spreadAngle;           // Угол разброса для дробовика (в градусах)
}
