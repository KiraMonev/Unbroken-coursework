using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class Exit : MonoBehaviour
{
    [SerializeField] private string nextSceneName;
    private Finish _finishManager;

    private void Awake()
    {
        _finishManager = FindObjectOfType<Finish>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Проверяем, что в триггер вошёл игрок
        if (!other.CompareTag("Player"))
            return;

        if (SceneManager.GetActiveScene().name == "Level 1")
        {
            ManagerLevel1 mgr = FindObjectOfType<ManagerLevel1>();
            if (mgr.CanProceed()) { 
                if (!string.IsNullOrEmpty(nextSceneName))
                {
                    SceneManager.LoadScene(nextSceneName);
                }
                else
                {
                    int currentIndex = SceneManager.GetActiveScene().buildIndex;
                    SceneManager.LoadScene(currentIndex + 1);
                }
            }
            else
            {
                Debug.Log("Нужно собрать ещё монеты, выйти нельзя");
            }
        }
        else if (SceneManager.GetActiveScene().name == "Level 2")
        {
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                int currentIndex = SceneManager.GetActiveScene().buildIndex;
                SceneManager.LoadScene(currentIndex + 1);
            }
        }
        else if (SceneManager.GetActiveScene().name == "Level 3") 
            _finishManager.ShowFinish();
    }
}
