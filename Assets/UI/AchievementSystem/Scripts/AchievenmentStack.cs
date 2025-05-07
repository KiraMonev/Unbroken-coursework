using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievenmentStack : MonoBehaviour
{
    public RectTransform[] StackPanels;
    public List<UIAchievement> BackLog = new List<UIAchievement>();

    public GameObject AchievementTemplate;
    private AchievementManager AM;

    private void Start()
    {
        AM = AchievementManager.instance;
    }

    public void ScheduleAchievementDisplay (int Index)
    {
        var Spawned = Instantiate(AchievementTemplate).GetComponent<UIAchievement>();
        Spawned.AS = this;
        Spawned.Set(AM.AchievementList[Index], AM.States[Index]);
        
        if (GetCurrentStack().childCount < AM.numberOnScreen)
        {
            Spawned.transform.SetParent(GetCurrentStack(), false);
            Spawned.StartDeathTimer();
        }
        else
        {
            Spawned.gameObject.SetActive(false);
            BackLog.Add(Spawned);
        }
    }

    public Transform GetCurrentStack () => StackPanels[(int)AM.stackLocation].transform;

    public void CheckBackLog ()
    {
        if(BackLog.Count > 0)
        {
            BackLog[0].transform.SetParent(GetCurrentStack(), false);
            BackLog[0].gameObject.SetActive(true);
            BackLog[0].StartDeathTimer();
            BackLog.RemoveAt(0);
        }
    }
}
