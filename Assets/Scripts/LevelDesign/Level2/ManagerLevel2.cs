using UnityEngine;
using UnityEngine.UI;

public class ManagerLevel2 : MonoBehaviour
{
    [Header("Timer UI")]
    [SerializeField]
    private Text timerText;

    [Header("Enemies Collected")]
    [SerializeField]
    private Text enemiesCountText;

    private float startTime;
    private bool timing;
    private int totalEnemies;
    private int killedEnemies;
    private float lastTotalTime;

    public float LastTotalTime => lastTotalTime;

    private void Start()
    {
        timing = false;
        killedEnemies = 0;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        totalEnemies = enemies.Length;
        UpdateTimerUI(0f);
        UpdateEnemiesUI();
    }

    private void Update()
    {
        if (timing)
        {
            float elapsed = Time.time - startTime;
            UpdateTimerUI(elapsed);
        }
    }

    private void UpdateTimerUI(float time)
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    private void UpdateEnemiesUI()
    {
        if (enemiesCountText != null)
        {
            enemiesCountText.text = string.Format("{0}/{1}", killedEnemies, totalEnemies);
        }
    }

    public void StartTimer()
    {
        if (!timing)
        {
            timing = true;
            startTime = Time.time;
            Debug.Log("[ManagerLevel2] Таймер запущен.");
        }
    }

    public void RegisterEnemyKilled()
    {
        killedEnemies++;
        UpdateEnemiesUI();
        Debug.LogFormat("[ManagerLevel2] Убито врагов: {0}/{1}", killedEnemies, totalEnemies);
    }

    public bool CanProceed()
    {
        return killedEnemies >= totalEnemies;
    }

    public void StopTimer()
    {
        if (!timing) return;
        timing = false;
        lastTotalTime = Time.time - startTime;
        Debug.LogFormat("[ManagerLevel2] Таймер остановлен: {0:F2} сек.", lastTotalTime);
    }
}