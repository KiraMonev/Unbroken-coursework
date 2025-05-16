using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class DeathScreenUI : MonoBehaviour
{
    public static DeathScreenUI Instance { get; private set; } // Добавьте эту строку
    [Header("Analytics UI")]

    public GameObject deathScreen;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timeText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Опционально, если экран должен сохраняться между сценами
        }
        else
        {
            Destroy(gameObject); // Удаляем дубликат
        }
    }

    private void Start()
    {
        deathScreen.SetActive(false);
    }

    public void ShowDeathScreen(float currentTime)
    {
        float averageTime = GameAnalytics.Instance.CalculateAveragePlayTime();
        string comparison = GetComparisonText(currentTime, averageTime);
        
        timeText.text = $"Ваше время: {currentTime:F1} сек\n" +
                       $"Среднее время: {averageTime:F1} сек\n" +
                       $"{comparison}";

        scoreText.text = "Score: " + ScoreManager.Instance.CurrentLevelScore;
        deathScreen.SetActive(true);
        Time.timeScale = 0f;
    }

    private string GetComparisonText(float current, float average)
    {
        if (average == 0) return "Это ваша первая игра!";
        
        float difference = current / average;
        
        if (difference > 1.1f) return "Дольше среднего!";
        if (difference < 0.9f) return "Быстрее среднего!";
        return "Как в среднем!";
    }

    public void OnOKButtonClicked()
    {
        deathScreen.SetActive(false);
        Time.timeScale = 1f;
    }
}
