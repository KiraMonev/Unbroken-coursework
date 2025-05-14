using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int health = 1;
    
    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        ScoreManager.Instance.AddKillScore();
        Destroy(gameObject);
    }
}