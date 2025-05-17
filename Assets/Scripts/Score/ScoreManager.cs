using UnityEngine;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int TotalScore { get; private set; }
    public int CurrentLevelScore { get; private set; }
    public int Diamonds { get; private set; }

    private Dictionary<int, bool> levelTasks = new Dictionary<int, bool>();
    private bool[] levelsCompleted = new bool[3];

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddKillScore()
    {
        AddScore(30);
    }

    public void AddDiamond()
    {
        Diamonds++;
        AddScore(10);
    }

    public void SpendDiamond(int amount)
    {
        Diamonds -= amount;
        SubtractScore(amount * 10);
    }

    public void CompleteLevelTask(int levelIndex)
    {
        if (!levelTasks.ContainsKey(levelIndex))
        {
            levelTasks[levelIndex] = true;
            AddScore(50);

            // Отмечаем уровень как пройденный
            if (levelIndex >= 0 && levelIndex < levelsCompleted.Length)
            {
                levelsCompleted[levelIndex] = true;

                // Проверяем все ли уровни пройдены
                if (CheckAllLevelsCompleted())
                {
                    AddScore(50);
                }
            }
        }
    }

    public void AddScore(int amount)
    {
        TotalScore += amount;
        CurrentLevelScore += amount;
    }

    public void SubtractScore(int amount)
    {
        TotalScore -= amount;
        CurrentLevelScore -= amount;
        if (TotalScore < 0) TotalScore = 0;
        if (CurrentLevelScore < 0) CurrentLevelScore = 0;
    }

    public void ResetCurrentLevelScore()
    {
        CurrentLevelScore = 0;
    }

    private bool CheckAllLevelsCompleted()
    {
        foreach (bool completed in levelsCompleted)
        {
            if (!completed) return false;
        }
        return true;
    }

    public int GetCurrentLevelIndex()
    {
        // Здесь должна быть логика определения текущего уровня
        // Это может быть через GameManager или SceneManager
        return 0; // Заглушка, нужно реализовать
    }

    public void ResetScore()
    {
        TotalScore = 0;
        CurrentLevelScore = 0;
        Diamonds = 0;
        for (int i = 0; i < levelsCompleted.Length; i++) {
            levelsCompleted[i] = false;
            levelTasks[i] = false;
        }
    }
}