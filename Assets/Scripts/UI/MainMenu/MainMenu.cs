using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void NewGame()
    {
        GameManager.instance.ResetProgress();
        SceneManager.LoadScene("Level 1"); 
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
