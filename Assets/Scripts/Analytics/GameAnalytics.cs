using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class GameAnalytics : MonoBehaviour
{
    private static GameAnalytics _instance;
    public static GameAnalytics Instance => _instance;

    private string filePath;
    private DateTime sessionStartTime;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        filePath = Path.Combine(Application.dataPath, "Scripts/Analytics/game_sessions.csv");
        InitializeFile();
    }

    private void InitializeFile()
    {
        if (!File.Exists(filePath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            File.WriteAllText(filePath, "SessionStart,PlayTime\n");
        }
    }

    public void StartNewSession()
    {
        sessionStartTime = DateTime.Now;
    }

    public void SaveSessionData(float playTime)
    {
        string data = $"{sessionStartTime:yyyy-MM-dd HH:mm:ss},{playTime:F1}\n";
        File.AppendAllText(filePath, data);
    }

    public float CalculateAveragePlayTime()
    {
        if (!File.Exists(filePath)) return 0f;

        string[] lines = File.ReadAllLines(filePath);
        if (lines.Length <= 1) return 0f; // Пропускаем заголовок

        float totalTime = 0f;
        int validSessions = 0;

        for (int i = 1; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split(',');
            if (parts.Length >= 2 && float.TryParse(parts[1], out float time))
            {
                totalTime += time;
                validSessions++;
            }
        }

        return validSessions > 0 ? totalTime / validSessions : 0f;
    }
}