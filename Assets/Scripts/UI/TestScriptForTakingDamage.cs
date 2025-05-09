using UnityEngine;

public class TestScriptForTakingDamage : MonoBehaviour
{
    private PlayerHealth _playerHealth;

    private void Awake()
    {
        _playerHealth = FindObjectOfType<PlayerHealth>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(" то то попал в шип");

        if (!other.CompareTag("Player"))
            return;

        _playerHealth.TakeDamage(1);
    }
}
