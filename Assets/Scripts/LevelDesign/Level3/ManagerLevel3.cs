using UnityEngine;
using UnityEngine.UI;

public class ManagerLevel3 : MonoBehaviour
{
    [Header("Timer UI")]
    [SerializeField]
    private Text timerText;

    [Header("Documents Collected")]
    [SerializeField]
    private Text docsCountText;

    private float startTime;
    private bool timing;
    private bool hasDocument;
    private int collectedDocs;

    private float lastTotalTime;

    // Возвращает время последнего прохождения (в секундах)
    public float LastTotalTime => lastTotalTime;

    private void Start()
    {
        timing = false;
        hasDocument = false;
        collectedDocs = 0;
        UpdateTimerUI(0f);
        UpdateDocsUI();
    }

    private void Update()
    {
        if (timing)
        {
            float elapsed = Time.time - startTime;
            UpdateTimerUI(elapsed);
        }
    }

    private void UpdateTimerUI(float time)
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    private void UpdateDocsUI()
    {
        if (docsCountText != null)
        {
            docsCountText.text = collectedDocs.ToString() + "/1";
        }
    }

    public void OnDocumentPickedUp()
    {
        if (!hasDocument)
        {
            hasDocument = true;
            collectedDocs = 1;
            UpdateDocsUI();
            Debug.Log("[ManagerLevel3] Документ подобран.");
        }
    }

    public void StartTimer()
    {
        if (!timing)
        {
            timing = true;
            startTime = Time.time;
            Debug.Log("[ManagerLevel3] Таймер запущен.");
        }
    }

    public bool HasDocument => hasDocument;

    public void StopTimerAndCheck()
    {
        if (!timing)
            return;

        timing = false;
        lastTotalTime = Time.time - startTime;
        Debug.LogFormat("[ManagerLevel3] Таймер остановлен: {0:F2} сек.", lastTotalTime);
    }
}