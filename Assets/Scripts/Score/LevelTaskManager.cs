using UnityEngine;

public class LevelTaskManager : MonoBehaviour
{
    [Header("Level 1 Task")]
    public bool usedGun = false;
    
    [Header("Level 2 Task")]
    public float timeLimit = 120f; // 2 минуты в секундах
    private float level2TimeElapsed = 0f;
    private int totalEnemies;
    private int killedEnemies;
    
    [Header("Level 3 Task")]
    public GameObject documents;
    public Transform goldenRoom;
    public float timeForMaxBonus = 120f; // 2 минуты
    public float timeForPartialBonus = 300f; // 5 минут
    private bool documentsDelivered = false;
    private float level3TimeElapsed = 0f;
    
    private void Start()
    {
        int currentLevel = ScoreManager.Instance.GetCurrentLevelIndex();
        
        if (currentLevel == 1) // Уровень 2 (индекс 1)
        {
            totalEnemies = GameObject.FindGameObjectsWithTag("Enemy").Length;
            InvokeRepeating("CheckLevel2Task", 1f, 1f);
        }
        else if (currentLevel == 2) // Уровень 3 (индекс 2)
        {
            InvokeRepeating("UpdateLevel3Timer", 1f, 1f);
        }
    }
    
    public void OnGunUsed()
    {
        if (ScoreManager.Instance.GetCurrentLevelIndex() == 0) // Уровень 1
        {
            usedGun = true;
        }
    }
    
    public void OnEnemyKilled()
    {
        if (ScoreManager.Instance.GetCurrentLevelIndex() == 1) // Уровень 2
        {
            killedEnemies++;
        }
    }
    
    private void CheckLevel2Task()
    {
        level2TimeElapsed++;
        
        if (killedEnemies >= totalEnemies && level2TimeElapsed <= timeLimit)
        {
            ScoreManager.Instance.CompleteLevelTask(1); // Уровень 2
            CancelInvoke("CheckLevel2Task");
        }
        else if (level2TimeElapsed > timeLimit)
        {
            CancelInvoke("CheckLevel2Task");
        }
    }
    
    public void DeliverDocuments()
    {
        if (ScoreManager.Instance.GetCurrentLevelIndex() == 2 && !documentsDelivered)
        {
            documentsDelivered = true;
            
            if (level3TimeElapsed <= timeForMaxBonus)
            {
                ScoreManager.Instance.AddScore(50);
            }
            else if (level3TimeElapsed <= timeForPartialBonus)
            {
                ScoreManager.Instance.AddScore(30);
            }
            
            ScoreManager.Instance.CompleteLevelTask(2); // Уровень 3
            CancelInvoke("UpdateLevel3Timer");
        }
    }
    
    private void UpdateLevel3Timer()
    {
        level3TimeElapsed++;
    }
    
    // Вызывается при завершении уровня
    public void OnLevelCompleted()
    {
        int currentLevel = ScoreManager.Instance.GetCurrentLevelIndex();
        
        if (currentLevel == 0 && !usedGun) // Уровень 1 - не использовал пистолет
        {
            ScoreManager.Instance.CompleteLevelTask(0);
        }
    }
}