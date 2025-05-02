using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct AchievementInformation
{
    [SerializeField] public string Key;
    [SerializeField] public string DisplayName;
    [SerializeField] public string Description;
    [SerializeField] public Sprite LockedIcon;
    [SerializeField] public bool LockOverlay;
    [SerializeField] public Sprite AchievedIcon;
    [SerializeField] public bool Spoiler;
    [SerializeField] public bool Progression;
    [SerializeField] public float ProgressGoal;
    [SerializeField] public float NotificationFrequency;
    [SerializeField] public string ProgressSuffix;
}

[System.Serializable]
public class AchievementState
{
    public AchievementState(float NewProgress, bool NewAchieved)
    {
        Progress = NewProgress;
        Achieved = NewAchieved;
    }
    public AchievementState() { }

    [SerializeField] public float Progress;
    [SerializeField] public int LastProgressUpdate = 0;
    [SerializeField] public bool Achieved = false;
}

public enum AchievementStackLocation
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
}