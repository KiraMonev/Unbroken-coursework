using UnityEngine;

public class LevelTaskManager : MonoBehaviour
{
    public static LevelTaskManager Instance { get; private set; }
    [Header("Level 1 Task")]
    public bool usedBullets = false;
    private bool level1BonusAwarded = false;
    
    [Header("Level 2 Task")]
    public float timeLimit = 120f;
    private float level2TimeElapsed = 0f;
    private int totalEnemies;
    private int killedEnemies;
    
    [Header("Level 3 Task")]
    public GameObject documents;
    public Transform goldenRoom;
    public float timeForMaxBonus = 120f;
    public float timeForPartialBonus = 300f;
    private bool documentsDelivered = false;
    private float level3TimeElapsed = 0f;
    
    private void Start()
    {
        int currentLevel = ScoreManager.Instance.GetCurrentLevelIndex();
        
        if (currentLevel == 0) // Level 1 initialization
        {
            usedBullets = false;
            level1BonusAwarded = false;
        }
        else if (currentLevel == 1) // Level 2
        {
            totalEnemies = GameObject.FindGameObjectsWithTag("Enemy").Length;
            InvokeRepeating("CheckLevel2Task", 1f, 1f);
        }
        else if (currentLevel == 2) // Level 3
        {
            InvokeRepeating("UpdateLevel3Timer", 1f, 1f);
        }
    }
    
    // Call this when enemy is killed by bullet
    public void RegisterBulletKill()
    {
        if (ScoreManager.Instance.GetCurrentLevelIndex() == 0 && !level1BonusAwarded)
        {
            usedBullets = true;
        }
    }
    
    public void OnEnemyKilled()
    {
        int currentLevel = ScoreManager.Instance.GetCurrentLevelIndex();
        
        if (currentLevel == 1) // Level 2
        {
            killedEnemies++;
        }
    }
    
    public void OnLevelCompleted()
    {
        int currentLevel = ScoreManager.Instance.GetCurrentLevelIndex();
        
        if (currentLevel == 0 && !usedBullets && !level1BonusAwarded)
        {
            ScoreManager.Instance.AddScore(50);
            ScoreManager.Instance.CompleteLevelTask(0);
            level1BonusAwarded = true;
            Debug.Log("Awarded 50 points for no bullet kills");
        }
    }
    
    private void CheckLevel2Task()
    {
        level2TimeElapsed++;
        
        if (killedEnemies >= totalEnemies && level2TimeElapsed <= timeLimit)
        {
            ScoreManager.Instance.CompleteLevelTask(1);
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
            
            ScoreManager.Instance.CompleteLevelTask(2);
            CancelInvoke("UpdateLevel3Timer");
        }
    }
    
    private void UpdateLevel3Timer()
    {
        level3TimeElapsed++;
    }
}