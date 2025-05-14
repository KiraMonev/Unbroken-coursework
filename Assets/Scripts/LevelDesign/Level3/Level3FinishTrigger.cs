using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Level3FinishTrigger : MonoBehaviour
{
    private ManagerLevel3 mgr;

    private void Awake()
    {
        Collider2D collider = GetComponent<Collider2D>();
        collider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        mgr = FindObjectOfType<ManagerLevel3>();
        if (mgr != null)
        {
            if (mgr.HasDocument)
            {
                mgr.StopTimerAndCheck();
            }
            else
            {
                Debug.Log("[Level3FinishTrigger] Сначала подберите документ.");
            }
        }
    }
}