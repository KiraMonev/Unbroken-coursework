using UnityEngine;
using UnityEngine.SceneManagement;

public class Finish : MonoBehaviour
{
    [SerializeField] private GameObject finishPanel;
    [SerializeField] private GameObject gameUI;

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ищем UI только что загруженной сцены
        var uiRoot = GameObject.Find("GameManager")?.transform.Find("UI");
        if (uiRoot != null)
        {
            finishPanel = uiRoot.Find("Finish").gameObject;
            gameUI = uiRoot.Find("GameStats").gameObject;
        }
    }

    public void ShowFinish()
    {
        if (finishPanel == null || gameUI == null)
        {
            Debug.LogError("UI-панели не найдены!");
            return;
        }
        finishPanel.SetActive(true);
        gameUI.SetActive(false);
        Time.timeScale = 0f;
    }

    public void HideFinish()
    {
        Time.timeScale = 1f;
        finishPanel.SetActive(false);
        gameUI.SetActive(true);
    }

    public void LoadLevel()
    {
        HideFinish();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenu()
    {
        HideFinish();
        SceneManager.LoadScene("MainMenu");
    }
}
