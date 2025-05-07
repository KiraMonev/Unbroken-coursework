using UnityEngine;
using System.IO;

public class GameStatsManager : MonoBehaviour {
    public static GameStatsManager Instance { get; private set; }

    public GameStats stats = new GameStats();
    private string filePath;

    void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        filePath = Application.persistentDataPath + "/game_stats.json";
        LoadStats();
    }

    void Update() {
        stats.AddPlayTime(Time.deltaTime);
    }

    public void SaveStats() {
        string json = JsonUtility.ToJson(stats, true);
        File.WriteAllText(filePath, json);
        Debug.Log("Game stats saved.");
    }

    public void LoadStats() {
        if (File.Exists(filePath)) {
            string json = File.ReadAllText(filePath);
            stats = JsonUtility.FromJson<GameStats>(json);
        } else {
            stats = new GameStats();
        }
    }

    void OnApplicationQuit() {
        SaveStats();
    }
}