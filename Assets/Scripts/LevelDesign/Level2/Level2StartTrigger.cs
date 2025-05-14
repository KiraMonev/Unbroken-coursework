using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Level2StartTrigger : MonoBehaviour
{
    private ManagerLevel2 mgr;

    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        mgr = FindObjectOfType<ManagerLevel2>();
        if (mgr != null)
        {
            mgr.StartTimer();
        }
        gameObject.SetActive(false);
    }
}