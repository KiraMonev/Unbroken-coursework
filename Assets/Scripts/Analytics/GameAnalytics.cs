using System;
using System.IO;
using UnityEngine;

public class GameAnalytics : MonoBehaviour
{
    public static GameAnalytics Instance { get; private set; }
    
    private string filePath;
    private DateTime sessionStartTime;
    private int enemiesKilledThisSession;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        filePath = Path.Combine(Application.dataPath, "Scripts/Analytics/game_sessions.csv");
        InitializeFile();
    }

    private void InitializeFile()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "SessionStart,PlayTime,EnemiesKilled\n");
        }
    }

    public void StartNewSession()
    {
        sessionStartTime = DateTime.Now;
        enemiesKilledThisSession = 0;
    }

    public void RegisterEnemyKill()
    {
        enemiesKilledThisSession++;
    }

    public int GetCurrentKills()
    {
        return enemiesKilledThisSession;
    }

    public void SaveSessionData(float playTime)
    {
        string data = $"{sessionStartTime:yyyy-MM-dd HH:mm:ss},{playTime:F1},{enemiesKilledThisSession}\n";
        File.AppendAllText(filePath, data);
    }

    public (float avgTime, float avgKills) GetAverages()
    {
        if (!File.Exists(filePath)) return (0f, 0f);

        string[] lines = File.ReadAllLines(filePath);
        if (lines.Length <= 1) return (0f, 0f);

        float totalTime = 0f;
        int totalKills = 0;
        int validSessions = 0;

        for (int i = 1; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split(',');
            if (parts.Length >= 3 && 
                float.TryParse(parts[1], out float time) && 
                int.TryParse(parts[2], out int kills))
            {
                totalTime += time;
                totalKills += kills;
                validSessions++;
            }
        }

        return validSessions > 0 
            ? (totalTime / validSessions, totalKills / (float)validSessions) 
            : (0f, 0f);
    }
}