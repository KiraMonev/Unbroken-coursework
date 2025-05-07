using UnityEngine;

public class Crystal : MonoBehaviour
{
    [Tooltip("Количество кристаллов, которое даёт этот объект")]
    [SerializeField] private int value = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CrystalManager.Instance.AddCrystal(value);
            Destroy(gameObject);
        }
    }
}
