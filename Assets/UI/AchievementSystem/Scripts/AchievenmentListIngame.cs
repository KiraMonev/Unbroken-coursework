﻿using UnityEngine;
using UnityEngine.UI;

public class AchievenmentListIngame : MonoBehaviour
{
    [HideInInspector] public GameObject scrollContent;
    [HideInInspector] public GameObject prefab;
    [HideInInspector] public GameObject Menu;
    [HideInInspector] public Dropdown Filter;
    [HideInInspector] public Text CountText;
    [HideInInspector] public Text CompleteText;
    [HideInInspector] public Scrollbar Scrollbar;

    public bool MenuOpen = false;
    public KeyCode OpenMenuKey; 
    private void AddAchievements(string Filter)
    {  
        foreach (Transform child in scrollContent.transform)
        {
            Destroy(child.gameObject);
        }
        AchievementManager AM = AchievementManager.instance;
        int AchievedCount = AM.GetAchievedCount();

        CountText.text = "" + AchievedCount + " / " + AM.States.Count;
        CompleteText.text = "Complete " + AM.GetAchievedPercentage() + "%";

        for (int i = 0; i < AM.AchievementList.Count; i ++)
        {
            if((Filter.Equals("All")) || (Filter.Equals("Achieved") && AM.States[i].Achieved) || (Filter.Equals("Unachieved") && !AM.States[i].Achieved))
            {
                AddAchievementToUI(AM.AchievementList[i], AM.States[i]);
            }
        }
        Scrollbar.value = 1;
    }

    public void AddAchievementToUI(AchievementInformation Achievement, AchievementState State)
    {
        UIAchievement UIAchievement = Instantiate(prefab, new Vector3(0f, 0f, 0f), Quaternion.identity).GetComponent<UIAchievement>();
        UIAchievement.Set(Achievement, State);
        UIAchievement.transform.SetParent(scrollContent.transform);
    }

    public void ChangeFilter ()
    {
        AddAchievements(Filter.options[Filter.value].text);
    }

    public void CloseWindow()
    {
        MenuOpen = false;
        Menu.SetActive(MenuOpen);
    }

    public void OpenWindow()
    {
        MenuOpen = true;
        Menu.SetActive(MenuOpen);
        AddAchievements("All");
    }

    public void ToggleWindow()
    {
        if (MenuOpen){
            CloseWindow();
        }
        else{
            OpenWindow();
        }
    }
 
    private void Update()
    {
        if(Input.GetKeyDown(OpenMenuKey))
        {
            ToggleWindow();
        }
    }
}