using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class DeathScreenUI : MonoBehaviour
{
    public static DeathScreenUI Instance { get; private set; } // Добавьте эту строку
    [Header("Analytics UI")]
    public Text keysText;
    public Text timeText;
    public Text weaponUsageText;

    public GameObject deathScreen;
    public TextMeshProUGUI scoreText;

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

    public void ShowDeathScreen()
    {
        scoreText.text = "Score: " + ScoreManager.Instance.CurrentLevelScore;
        deathScreen.SetActive(true);
        Time.timeScale = 0f;
    }

    public void OnOKButtonClicked()
    {
        deathScreen.SetActive(false);
        Time.timeScale = 1f;
    }

    // public void ShowAnalytics()
    // {
    //     // 1. Анализ клавиш
    //     var currentKeys = GameAnalytics.Instance.currentSession.keyPresses;
    //     var allKeys = GameAnalytics.Instance.allSessions
    //         .SelectMany(s => s.keyPresses)
    //         .GroupBy(k => k.Key)
    //         .ToDictionary(g => g.Key, g => g.Sum(k => k.Value));

    //     KeyCode mostUsedKeyCurrent = currentKeys.OrderByDescending(k => k.Value).First().Key;
    //     KeyCode mostUsedKeyAll = allKeys.OrderByDescending(k => k.Value).First().Key;
        
    //     keysText.text = $"Чаще всего нажимали: {mostUsedKeyCurrent}\n" +
    //                    $"Совпадает с общим: {(mostUsedKeyCurrent == mostUsedKeyAll ? "Да" : "Нет")}";

    //     // 2. Анализ времени
    //     float currentLevelTime = GameAnalytics.Instance.currentSession.levelTimes.Values.Last();
    //     float avgTime = GameAnalytics.Instance.allSessions
    //         .Where(s => s.levelTimes.ContainsKey(currentLevelIndex))
    //         .Average(s => s.levelTimes[currentLevelIndex]);
        
    //     string timeComparison = currentLevelTime switch {
    //         var t when t < avgTime * 0.9f => "быстрее среднего",
    //         var t when t > avgTime * 1.1f => "медленнее среднего",
    //         _ => "среднее"
    //     };
        
    //     timeText.text = $"Время: {currentLevelTime:F1} сек ({timeComparison})";

    //     // 3. Использование оружия
    //     int currentClicks = GameAnalytics.Instance.currentSession.mouseClicks;
    //     float avgClicks = GameAnalytics.Instance.allSessions
    //         .Average(s => s.mouseClicks);
        
    //     string clicksComparison = currentClicks switch {
    //         var c when c < avgClicks * 0.7f => "реже",
    //         var c when c > avgClicks * 1.3f => "чаще",
    //         _ => "как в среднем"
    //     };
        
    //     weaponUsageText.text = $"Использование оружия: {clicksComparison}";
    // }
}
