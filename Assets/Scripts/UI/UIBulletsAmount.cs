using UnityEngine;
using UnityEngine.UI;

public class UIBulletsAmount : MonoBehaviour
{
    public int currentAmmo;
    public int totalAmmo;
    public Text ammoDisplay;

    void Update()
    {
        ammoDisplay.text = $"{currentAmmo} / {totalAmmo}";
    }

    public void SetTotalAmmo(int newTotalAmmo)
    {
        totalAmmo = newTotalAmmo;
    }

    public void SetCurrentAmmo(int newCurrentAmmo)
    {
        currentAmmo = newCurrentAmmo;
    }
}
