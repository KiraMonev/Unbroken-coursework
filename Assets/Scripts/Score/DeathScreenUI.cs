using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeathScreenUI : MonoBehaviour
{
    public static DeathScreenUI Instance { get; private set; } // Добавьте эту строку

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
}