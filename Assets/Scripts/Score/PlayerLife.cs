using UnityEngine;

public class PlayerLife : MonoBehaviour
{
    public int health = 1;
    public DeathScreenUI deathScreen;
    
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
        deathScreen.ShowDeathScreen();
        // Дополнительная логика смерти игрока
    }
}