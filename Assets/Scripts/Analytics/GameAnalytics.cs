using System;
using System.IO;
using UnityEngine;

public class GameAnalytics : MonoBehaviour
{
    public static GameAnalytics Instance;
    
    private string sessionStartTime;
    private string analyticsPath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            analyticsPath = Application.dataPath + "/Scripts/Analytics/game_stats.txt";
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartNewSession()
    {
        sessionStartTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

    public void SaveSessionData(string deathReason)
    {
        TimeSpan playTime = DateTime.Now - DateTime.Parse(sessionStartTime);
        
        string data = $"{sessionStartTime},{playTime.TotalSeconds:F1},{deathReason}\n";
        
        File.AppendAllText(analyticsPath, data);
    }

    public float GetAveragePlayTime()
    {
        if (!File.Exists(analyticsPath)) return 0;

        string[] allLines = File.ReadAllLines(analyticsPath);
        if (allLines.Length == 0) return 0;

        float totalTime = 0;
        foreach (string line in allLines)
        {
            string[] parts = line.Split(',');
            if (parts.Length >= 2 && float.TryParse(parts[1], out float time))
            {
                totalTime += time;
            }
        }

        return totalTime / allLines.Length;
    }
}