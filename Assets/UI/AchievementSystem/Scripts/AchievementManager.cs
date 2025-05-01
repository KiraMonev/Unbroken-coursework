using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;


[System.Serializable]
public class AchievementManager : MonoBehaviour
{
    public float displayTime = 3; // Время отображения иконки достижения
    public int numberOnScreen = 3; // Количество уведомлений о достижениях на экране
    public bool showExactProgress = false;
    public bool displayAchievements;
    public AchievementStackLocation stackLocation; // Место отображения иконки достижения
    public bool autoSave;
    public string spoilerAchievementMessage = "Hidden";
    public AudioClip achievedSound; // Звук при получении достижения
    public AudioClip progressMadeSound; // Звук при продвижении прогресса достижения
    
    private AudioSource AudioSource; // "Колонка" через которую воспроизводится звук
   
    [SerializeField] public List<AchievementState> States = new List<AchievementState>();                       // Список состояний
    [SerializeField] public List<AchievementInformation> AchievementList = new List<AchievementInformation>();  // Список достижений

    public bool useFinalAchievement = false;
    public string finalAchievementKey;

    public static AchievementManager instance = null;
    public AchievenmentStack Stack;

    void Awake()
    {
       if (instance == null)
       {
            instance = this;
       }
       else if (instance != this)
       {
            Destroy(gameObject);
       }
        //DontDestroyOnLoad(gameObject);
        AudioSource = gameObject.GetComponent<AudioSource>();
        Stack = GetComponentInChildren<AchievenmentStack>();
        LoadAchievementState();
    }

    private void PlaySound (AudioClip Sound)
    {
        if(AudioSource != null)
        {
            AudioSource.clip = Sound;
            AudioSource.Play();
        }
    }
    # region Miscellaneous
    public bool AchievementExists(string Key)
    {
        return AchievementExists(AchievementList.FindIndex(x => x.Key.Equals(Key)));
    }
    public bool AchievementExists(int Index)
    {
        return Index <= AchievementList.Count && Index >= 0;
    }
    public int GetAchievedCount()
    {
        int Count = (from AchievementState i in States
                    where i.Achieved == true
                    select i).Count();
        return Count;
    }
    public float GetAchievedPercentage()
    {
        if(States.Count == 0)
        {
            return 0;
        }
        return (float)GetAchievedCount() / States.Count * 100;
    }
    #endregion

    #region Unlock and Progress
    public void Unlock(string Key)
    {
        Unlock(FindAchievementIndex(Key));
    }

    public void Unlock(int Index)
    {
        if (!States[Index].Achieved)
        {
            States[Index].Progress = AchievementList[Index].ProgressGoal;
            States[Index].Achieved = true;
            DisplayUnlock(Index);
            AutoSaveStates();

            if(useFinalAchievement)
            {
                int Find = States.FindIndex(x => !x.Achieved);
                bool CompletedAll = (Find == -1 || AchievementList[Find].Key.Equals(finalAchievementKey));
                if (CompletedAll)
                {
                    Unlock(finalAchievementKey);
                }
            }
        }
    }
    public void SetAchievementProgress(string Key, float Progress)
    {
        SetAchievementProgress(FindAchievementIndex(Key), Progress);
    }
    public void SetAchievementProgress(int Index, float Progress)
    {
        if(AchievementList[Index].Progression)
        {
            if (States[Index].Progress >= AchievementList[Index].ProgressGoal)
            {
                Unlock(Index);
            }
            else
            {
                States[Index].Progress = Progress;
                DisplayUnlock(Index);
                AutoSaveStates();                
            }
        }
    }
    public void AddAchievementProgress(string Key, float Progress)
    {
        AddAchievementProgress(FindAchievementIndex(Key), Progress);
    }
    public void AddAchievementProgress(int Index, float Progress)
    {
        if (AchievementList[Index].Progression)
        {
            if (States[Index].Progress + Progress >= AchievementList[Index].ProgressGoal)
            {
                Unlock(Index);
            }
            else
            {
                States[Index].Progress += Progress;
                DisplayUnlock(Index);
                AutoSaveStates();
            }
        }
    }
    #endregion

    #region Saving and Loading
    public void SaveAchievementState()
    {
        for (int i = 0; i < States.Count; i++)
        {
            PlayerPrefs.SetString("AchievementState_" + i, JsonUtility.ToJson(States[i]));
        }
        PlayerPrefs.Save();
    }
    public void LoadAchievementState()
    {
        AchievementState NewState;
        States.Clear();

        for (int i = 0; i < AchievementList.Count; i++)
        {
            if (PlayerPrefs.HasKey("AchievementState_" + i))
            {
                NewState = JsonUtility.FromJson<AchievementState>(PlayerPrefs.GetString("AchievementState_" + i));
                States.Add(NewState);
            }
            else { States.Add(new AchievementState()); }
            
        }
    }
    public void ResetAchievementState()
    {
        States.Clear();
        for (int i = 0; i < AchievementList.Count; i++)
        {
            PlayerPrefs.DeleteKey("AchievementState_" + i);
            States.Add(new AchievementState());
        }
        SaveAchievementState();
    }
    #endregion


    private int FindAchievementIndex(string Key)
    {
        return AchievementList.FindIndex(x => x.Key.Equals(Key));
    }
    private void AutoSaveStates()
    {
        if (autoSave)
        {
            SaveAchievementState();
        }
    }
    private void DisplayUnlock(int Index)
    {
        if (displayAchievements && !AchievementList[Index].Spoiler || States[Index].Achieved)
        {
            // Если не открыто
            if (AchievementList[Index].Progression && States[Index].Progress < AchievementList[Index].ProgressGoal)
            {
                int Steps = (int)AchievementList[Index].ProgressGoal / (int)AchievementList[Index].NotificationFrequency;

                for (int i = Steps; i > States[Index].LastProgressUpdate; i--)
                {
                   if (States[Index].Progress >= AchievementList[Index].NotificationFrequency * i)
                   {
                        PlaySound(progressMadeSound);
                        States[Index].LastProgressUpdate = i;
                        Stack.ScheduleAchievementDisplay(Index);
                        return;
                   }
                }
            }
            else
            {
                PlaySound(achievedSound);
                Stack.ScheduleAchievementDisplay(Index);
            }
        }
    }
}