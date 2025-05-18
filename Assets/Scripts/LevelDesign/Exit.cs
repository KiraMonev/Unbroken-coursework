using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class Exit : MonoBehaviour
{
    [SerializeField] private string nextSceneName;
    private Finish _finishManager;

    [SerializeField] private float shortTime = 50f;
    [SerializeField] private int scoreForLongTime = 30;
    [SerializeField] private int scoreForShortTime = 50;

    private void Awake()
    {
        _finishManager = FindObjectOfType<Finish>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "Level 1")
        {
            ManagerLevel1 mgr1 = FindObjectOfType<ManagerLevel1>();
            if (mgr1.CanProceed()) LoadNext();
            else Debug.Log("Need more digits");
        }
        else if (sceneName == "Level 2")
        {
            ManagerLevel2 mgr2 = FindObjectOfType<ManagerLevel2>();
            if (mgr2.CanProceed())
            {
                mgr2.StopTimer();

                if (mgr2.LastTotalTime <= shortTime) { 
                    ScoreManager.Instance.AddScore(scoreForShortTime);
                    AchievementManager.instance.Unlock("level2");
                }
                else
                    ScoreManager.Instance.AddScore(scoreForLongTime);

                Debug.LogFormat("[Exit] Level 2 complete in {0:F2} sec.", mgr2.LastTotalTime);
                LoadNext();
            }
            else
            {
                Debug.Log("[Exit] Cannot exit: some enemies remain.");
            }
        }
        else if (sceneName == "Level 3")
        {
            _finishManager.ShowFinish();
        }
    }

    private void LoadNext()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}