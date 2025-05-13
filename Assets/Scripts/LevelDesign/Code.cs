using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Code : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ManagerLevel1 mgr = FindObjectOfType<ManagerLevel1>();
            if (mgr != null)
            {
                mgr.RegisterDigitCollected();
            }
            Destroy(gameObject);
        }
    }
}
