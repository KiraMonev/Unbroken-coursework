using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathMenu : MonoBehaviour
{
    [SerializeField] public GameObject deathMenu;
    public static DeathMenu Instance;
    [SerializeField] public GameObject gameUI;
    private PlayerHealth _playerHealth;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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
        FindUIElements();
        if (_playerHealth.isDead)
        {
            HideDeathMenu();
        }
        else
        {
            gameUI.SetActive(true);
            deathMenu.SetActive(false);
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
                deathMenu = uiTransform.Find("DeathMenu").gameObject;
                gameUI = uiTransform.Find("GameStats").gameObject;
            }
        }
    }

    public void HideDeathMenu()
    {
        Time.timeScale = 1f;
        deathMenu.SetActive(false);
        gameUI.SetActive(true);
        _playerHealth.SetFullHealth();
    }

    public void ShowDeathMenu()
    {
        deathMenu.SetActive(true);
        gameUI.SetActive(false);
        Time.timeScale = 0f;
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