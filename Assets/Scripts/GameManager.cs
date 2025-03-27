using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private int lastSceneIndex;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ����� ��� ���������� ���������
    public void SaveGame()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            PlayerPrefs.SetInt("LastSceneIndex", SceneManager.GetActiveScene().buildIndex + 1);
        }
        PlayerPrefs.Save();
    }

    // ����� ��� �������� ���������
    public void LoadGame()
    {
        lastSceneIndex = PlayerPrefs.GetInt("LastSceneIndex", 0);
        SceneManager.LoadScene(lastSceneIndex);
    }

    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey("LastSceneIndex");
    }
}
