using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class GameAnalytics : MonoBehaviour
{
    public static GameAnalytics Instance { get; private set; }

    // Данные для хранения
    public class SessionData
    {
        public Dictionary<KeyCode, int> keyPresses = new Dictionary<KeyCode, int>();
        public Dictionary<int, float> levelTimes = new Dictionary<int, float>();
        public int mouseClicks;
        public int sessionNumber;
        public string deathLevel;
    }

    private SessionData currentSession;
    private List<SessionData> allSessions = new List<SessionData>();
    private float levelStartTime;
    private int currentLevelIndex;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartNewSession(int levelIndex)
    {
        currentSession = new SessionData();
        currentLevelIndex = levelIndex;
        levelStartTime = Time.time;
    }

    public void RecordKeyPress(KeyCode key)
    {
        if (!currentSession.keyPresses.ContainsKey(key))
            currentSession.keyPresses[key] = 0;
        
        currentSession.keyPresses[key]++;
    }

    public void RecordMouseClick()
    {
        currentSession.mouseClicks++;
    }

    public void CompleteLevel(int levelIndex)
    {
        float time = Time.time - levelStartTime;
        currentSession.levelTimes[levelIndex] = time;
        levelStartTime = Time.time;
    }

    public void EndSession(string deathLevel)
    {
        currentSession.deathLevel = deathLevel;
        currentSession.sessionNumber = allSessions.Count + 1;
        allSessions.Add(currentSession);
        SaveData();
    }

    private void SaveData()
    {
        string path = Application.persistentDataPath + "/analytics.json";
        string json = JsonUtility.ToJson(new SessionCollection(allSessions));
        File.WriteAllText(path, json);
    }

    private void LoadData()
    {
        string path = Application.persistentDataPath + "/analytics.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            allSessions = JsonUtility.FromJson<SessionCollection>(json).sessions;
        }
    }

    [System.Serializable]
    private class SessionCollection
    {
        public List<SessionData> sessions;
        public SessionCollection(List<SessionData> data) => sessions = data;
    }
}