using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] public GameObject pauseMenu;
    public static PauseMenu Instance;
    [SerializeField] public GameObject gameUI;
    public bool isPaused = false;
    private AchievenmentListIngame _achievementList;

    private void Awake()
    {
        // ≈сли это первый экземпл€р Ч сохран€ем и инициализируем
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _achievementList = FindObjectOfType<AchievenmentListIngame>();
        }
        else
        {
            Destroy(gameObject);
        }

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
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
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
