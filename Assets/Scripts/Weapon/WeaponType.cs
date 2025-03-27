using System;
using UnityEngine;

[Serializable]
public enum WeaponType
{
    [InspectorName("Без оружия")] NoWeapon = 0,
    [InspectorName("Нож")] Knife = 1,
    [InspectorName("Катана")] Katana = 2,
    [InspectorName("Бита")] Ballbat = 3,
    [InspectorName("Пистолет")] Pistol = 4,
    [InspectorName("Узи")] Uzi = 5,
    [InspectorName("Винтовка")] Rifle = 6,
    [InspectorName("Дробовик")] Shotgun = 7,
}
