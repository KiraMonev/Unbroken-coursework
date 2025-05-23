using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] public GameObject pauseMenu;
    public static PauseMenu Instance;
    [SerializeField] public GameObject gameUI;
    public bool isPaused = false;
    private AchievenmentListIngame _achievementList;

    private PlayerHealth _playerHealth;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _achievementList = FindObjectOfType<AchievenmentListIngame>();
            _playerHealth = FindObjectOfType<PlayerHealth>();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            Destroy(gameObject);
            return;
        }

        FindUIElements();
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            gameUI.SetActive(true);
            pauseMenu.SetActive(false);
        }
    }

    private void FindUIElements()
    {
        GameObject gameManager = GameObject.Find("GameManager");
        if (gameManager != null)
        {
            Transform uiTransform = gameManager.transform.Find("UI");
            if (uiTransform != null)
            {
                pauseMenu = uiTransform.Find("PauseMenu").gameObject;
                gameUI = uiTransform.Find("GameStats").gameObject;
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !_playerHealth.isDead)
        {
            if (isPaused && !_achievementList.MenuOpen)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
        gameUI.SetActive(true);
        isPaused = false;
    }

    void PauseGame()
    {
        pauseMenu.SetActive(true);
        gameUI.SetActive(false);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void LoadLevel()
    {
        Time.timeScale = 1f;
        ScoreManager.Instance.ResetScore();
        PlayerController.Instance.ResetPlayer();
        AchievementManager.instance.ResetAchievementState();
        SceneManager.LoadScene("Level 1");
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        gameUI.SetActive(false);
        SceneManager.LoadScene("MainMenu");
    }
}