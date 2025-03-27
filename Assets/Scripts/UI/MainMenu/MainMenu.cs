using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void NewGame()
    {
        GameManager.instance.ResetProgress();
        SceneManager.LoadScene(0); 
    }

    public void ContinueGame()
    {
        GameManager.instance.LoadGame(); 
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
