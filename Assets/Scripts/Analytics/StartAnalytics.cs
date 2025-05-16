using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartAnalytics : MonoBehaviour
{
    void Start()
    {
        GameAnalytics.Instance.StartNewSession();
    }
}
