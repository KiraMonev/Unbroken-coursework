using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

public class GameAnalytics : MonoBehaviour
{
    public static GameAnalytics Instance { get; private set; }

    private string filePath;
    private DateTime sessionStartTime;
    private int enemiesKilledThisSession;
    private Dictionary<string, int> buttonPresses;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        buttonPresses = new Dictionary<string, int>();

        filePath = Path.Combine(Application.dataPath, "Scripts/Analytics/game_sessions.csv");
        InitializeFile();
    }

    private void InitializeFile()
    {
        // Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        // if (!File.Exists(filePath))
        // {
        //     File.WriteAllText(filePath, "SessionStart,PlayTime,EnemiesKilled\n");
        // }
        if (!File.Exists(filePath))
        {
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine("SessionStartTime, PlayTime, EnemiesKilled, ButtonPresses");
            }
        }
    }

    public void StartNewSession()
    {
        sessionStartTime = DateTime.Now;
        enemiesKilledThisSession = 0;
        buttonPresses.Clear();
    }

    public void RegisterButtonPress(string buttonName)
    {
        if (buttonPresses.ContainsKey(buttonName))
        {
            buttonPresses[buttonName]++;
        }
        else
        {
            buttonPresses[buttonName] = 1;
        }
    }

    public Dictionary<string, int> GetButtonPresses()
    {
        return buttonPresses;
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
        string data = $"{sessionStartTime:yyyy-MM-dd HH:mm:ss},{playTime:F1},{enemiesKilledThisSession}";
        foreach (var entry in buttonPresses)
        {
            data += $", {entry.Key}:{entry.Value}";
        }
        data += "\n";
        File.AppendAllText(filePath, data);
    }

    public float GetAveragePlayTime()
    {
        if (!File.Exists(filePath)) return 0f;

        string[] lines = File.ReadAllLines(filePath);
        if (lines.Length <= 1) return 0f;

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

    public float GetAverageKills()
    {
        if (!File.Exists(filePath)) return 0f;

        string[] lines = File.ReadAllLines(filePath);
        if (lines.Length <= 1) return 0f;

        float totalKills = 0f;
        int validSessions = 0;

        for (int i = 1; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split(',');
            if (parts.Length >= 3 && int.TryParse(parts[2], out int kills))
            {
                totalKills += kills;
                validSessions++;
            }
        }

        return validSessions > 0 ? totalKills / validSessions : 0f;
    }

    public Dictionary<string, float> CalculateAverageButtonPresses()
    {
        Dictionary<string, int> totalButtonPresses = new Dictionary<string, int>();
        Dictionary<string, int> sessionCounts = new Dictionary<string, int>();

        // Читаем данные из файла
        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);
            
            // Пропускаем заголовок
            for (int i = 1; i < lines.Length; i++)
            {
                string[] parts = lines[i].Split(',');

                // Обрабатываем нажатия кнопок
                for (int j = 3; j < parts.Length; j++)
                {
                    string[] buttonData = parts[j].Split(':');
                    if (buttonData.Length == 2)
                    {
                        string buttonName = buttonData[0].Trim();
                        int pressCount = int.Parse(buttonData[1].Trim());

                        if (!totalButtonPresses.ContainsKey(buttonName))
                        {
                            totalButtonPresses[buttonName] = 0;
                            sessionCounts[buttonName] = 0;
                        }

                        totalButtonPresses[buttonName] += pressCount;
                        sessionCounts[buttonName]++;
                    }
                }
            }
        }

        // Вычисляем средние значения
        Dictionary<string, float> averageButtonPresses = new Dictionary<string, float>();
        foreach (var entry in totalButtonPresses)
        {
            averageButtonPresses[entry.Key] = (float)entry.Value / sessionCounts[entry.Key];
        }

        return averageButtonPresses;
    }
}