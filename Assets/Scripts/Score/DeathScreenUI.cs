using UnityEngine;
using TMPro;

public class DeathScreenUI : MonoBehaviour
{
    public static DeathScreenUI Instance;

    [Header("UI References")]
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text killsText;
    [SerializeField] private TMP_Text scoreText;

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

            timeText.text = $"Время игры: {currentTime:F1} сек\n" +
                           $"Среднее: {avgTime:F1} сек";

            killsText.text = $"Убито врагов: {currentKills}\n" +
                             $"Среднее: {avgKills:F1}";
            
            scoreText.text = "Score: " + ScoreManager.Instance.CurrentLevelScore;

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
