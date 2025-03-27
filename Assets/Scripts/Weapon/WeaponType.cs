using System;
using UnityEngine;

[Serializable]
public enum WeaponType
{
    [InspectorName("��� ������")] NoWeapon = 0,
    [InspectorName("���")] Knife = 1,
    [InspectorName("������")] Katana = 2,
    [InspectorName("����")] Ballbat = 3,
    [InspectorName("��������")] Pistol = 4,
    [InspectorName("���")] Uzi = 5,
    [InspectorName("��������")] Rifle = 6,
    [InspectorName("��������")] Shotgun = 7,
}
