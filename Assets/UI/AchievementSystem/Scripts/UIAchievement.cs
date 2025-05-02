using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIAchievement : MonoBehaviour
{
    public Text Title, Description, Percent;
    public Image Icon, OverlayIcon, ProgressBar;
    public GameObject SpoilerOverlay;
    public Text SpoilerText;
    [SerializeField] private Font font;
    [HideInInspector]public AchievenmentStack AS;


    public void StartDeathTimer ()
    {
        StartCoroutine(Wait());
    }


    public void Set (AchievementInformation Information, AchievementState State)
    {
        if(Information.Spoiler && !State.Achieved)
        {
            SpoilerOverlay.SetActive(true);
            SpoilerText.text = AchievementManager.instance.spoilerAchievementMessage;
            SpoilerText.font = font;
        }
        else
        {
            Title.text = Information.DisplayName;
            Title.font = font;
            Description.text = Information.Description;
            Description.font = font;
            Percent.font = font;

            if (Information.LockOverlay && !State.Achieved)
            {
                OverlayIcon.gameObject.SetActive(true);
                OverlayIcon.sprite = Information.LockedIcon;
                Icon.sprite = Information.AchievedIcon;
            }
            else
            {
                Icon.sprite = State.Achieved ? Information.AchievedIcon : Information.LockedIcon;
            }

            if (Information.Progression)
            {
                float CurrentProgress = AchievementManager.instance.showExactProgress ? State.Progress : (State.LastProgressUpdate * Information.NotificationFrequency);
                float DisplayProgress = State.Achieved ? Information.ProgressGoal : CurrentProgress;

                if (State.Achieved)
                {
                    Percent.text = Information.ProgressGoal + Information.ProgressSuffix + " / " + Information.ProgressGoal + Information.ProgressSuffix + " Achieved";
                }
                else
                {
                    Percent.text = DisplayProgress + Information.ProgressSuffix +  " / " + Information.ProgressGoal + Information.ProgressSuffix;
                }

                ProgressBar.fillAmount = DisplayProgress / Information.ProgressGoal;
            }
            else
            {
                ProgressBar.fillAmount = State.Achieved ? 1 : 0;
                Percent.text = State.Achieved ? "Achieved" : "Locked";
            }
        }
    }

    private IEnumerator Wait ()
    {
        yield return new WaitForSeconds(AchievementManager.instance.displayTime);
        GetComponent<Animator>().SetTrigger("ScaleDown");
        yield return new WaitForSeconds(0.1f);
        AS.CheckBackLog();
        Destroy(gameObject);
    }
}
