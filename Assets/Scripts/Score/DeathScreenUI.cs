using UnityEngine;
using TMPro;
using System;

public class DeathScreenUI : MonoBehaviour
{
    public static DeathScreenUI Instance;

    [Header("UI References")]
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text killsText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text keysText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Важное изменение - сразу отключаем панель
            if (deathPanel != null)
            {
                deathPanel.SetActive(false);
            }
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowDeathScreen(float currentTime, int currentKills)
    {
        // Дополнительная проверка на null
        if (deathPanel == null || timeText == null || killsText == null)
        {
            Debug.LogError("UI references not assigned in DeathScreenUI");
            return;
        }

        if (!deathPanel.activeSelf)
        {
            float avgTime = GameAnalytics.Instance.GetAveragePlayTime();
            float avgKills = GameAnalytics.Instance.GetAverageKills();
            var buttonPresses = GameAnalytics.Instance.GetButtonPresses();
            var averageButtonPresses = GameAnalytics.Instance.CalculateAverageButtonPresses();
            string timeComparison = currentTime > avgTime ? "больше" : (currentTime < avgTime ? "меньше" : "равно");
            string killsComparison = currentKills > avgKills ? "больше" : (currentKills < avgKills ? "меньше" : "равно");
            string buttonPressText = "Нажатия кнопок:\n";
            foreach (var entry in buttonPresses)
            {
                buttonPressText += $"{entry.Key}: {entry.Value}\n";
            }

            // Оценка понимания управления
            int closeToAverageCount = 0;
            foreach (var entry in buttonPresses)
            {
                if (averageButtonPresses.ContainsKey(entry.Key))
                {
                    // Проверяем, приближено ли значение к среднему
                    if (Math.Abs(entry.Value - averageButtonPresses[entry.Key]) <= 10) // Порог в 10
                    {
                        closeToAverageCount++;
                    }
                }
            }

            string controlUnderstanding = closeToAverageCount >= 3 ? "Игрок понимает управление." : "Игроку нужно больше практики.";

            timeText.text = $"Время игры: {currentTime:F1} сек\n" +
                           $"Среднее: {avgTime:F1} сек\n" +
                           $"{timeComparison} среднего\n";

            killsText.text = $"Убито врагов: {currentKills}\n" +
                             $"Среднее: {avgKills:F1}\n" +
                             $"{killsComparison} среднего\n";

            scoreText.text = $"Score: {ScoreManager.Instance.CurrentLevelScore}\n";
            keysText.text = $"{buttonPressText}\n" +
                            $"{controlUnderstanding}\n";

            deathPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void HideDeathScreen()
    {
        if (deathPanel != null && deathPanel.activeSelf)
        {
            deathPanel.SetActive(false);
            Time.timeScale = 0f;
        }
    }

    public void OnOKButtonClicked()
    {
        deathPanel.SetActive(false);
    }
}
