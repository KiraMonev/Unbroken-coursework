using UnityEngine;
using UnityEngine.UI;

public class DeathScreenUI : MonoBehaviour
{
    public GameObject deathScreen;
    public Text scoreText;
    
    private void Start()
    {
        deathScreen.SetActive(false);
    }
    
    public void ShowDeathScreen()
    {
        scoreText.text = "Score: " + ScoreManager.Instance.CurrentLevelScore;
        deathScreen.SetActive(true);
        Time.timeScale = 0f; // Пауза игры
    }
    
    public void OnOKButtonClicked()
    {
        deathScreen.SetActive(false);
        Time.timeScale = 1f; // Возобновление игры
        // Здесь может быть переход в меню или рестарт уровня
    }
}