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
    public TextMeshProUGUI killsText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        deathScreen.SetActive(false);
    }

    public void ShowDeathScreen(float currentTime, int currentKills)
    {
        var (avgTime, avgKills) = GameAnalytics.Instance.GetAverages();
        
        string timeComparison = GetComparisonText(currentTime, avgTime, "время");
        string killsComparison = GetComparisonText(currentKills, avgKills, "убийства");

        timeText.text = $"Время игры: {currentTime:F1} сек ({timeComparison})\n" +
                       $"Среднее время: {avgTime:F1} сек\n\n";

        killsText.text = $"Убито врагов: {currentKills} ({killsComparison})\n" +
                         $"Среднее количество: {avgKills:F1}";

        scoreText.text = "Score: " + ScoreManager.Instance.CurrentLevelScore;
        deathScreen.SetActive(true);
        Time.timeScale = 0f;
    }

    private string GetComparisonText(float current, float average, string statName)
    {
        if (average == 0) return $"первый раз ({statName})";
        
        float difference = current / average;
        
        if (difference > 1.1f) return $"больше среднего ({statName})";
        if (difference < 0.9f) return $"меньше среднего ({statName})";
        return $"как в среднем ({statName})";
    }

    public void OnOKButtonClicked()
    {
        deathScreen.SetActive(false);
        Time.timeScale = 1f;
    }
}
