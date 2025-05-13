using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DocumentPickup : MonoBehaviour
{
    private ManagerLevel3 mgr;
    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        mgr = FindObjectOfType<ManagerLevel3>();
        if (mgr != null)
        {
            mgr.OnDocumentPickedUp();
        }

        Destroy(gameObject);
    }
}