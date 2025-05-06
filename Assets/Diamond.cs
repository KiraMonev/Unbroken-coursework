using UnityEngine;

public class Crystal : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            DiamondManager.instance.AddCrystal();
            Destroy(gameObject); // Удаляем кристалл после сбора
        }
    }
}
