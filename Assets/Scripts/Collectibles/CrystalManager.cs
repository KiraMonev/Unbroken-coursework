using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CrystalManager : MonoBehaviour
{
    public static CrystalManager Instance { get; private set; }

    [Tooltip("Текстовый элемент UI для отображения количества кристаллов")]
    [SerializeField] private Text crystalText;

    private int crystalCount = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            GameAnalytics.Instance.StartNewSession();
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start() => UpdateUI();

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var textGO = GameObject.Find("CrystalAmount");
        if (textGO != null)
            crystalText = textGO.GetComponent<Text>();

        UpdateUI(); 
    }

    public void AddCrystal(int amount)
    {
        crystalCount += amount;
        UpdateUI();
        AchievementManager.instance.Unlock("crystal");
    }

    public bool SpendCrystal(int amount)
    {
        if (crystalCount >= amount)
        {
            crystalCount -= amount;
            UpdateUI();
            return true;
        }
        else
        {
            return false;
        }
    }

    private void UpdateUI()
    {
        if (crystalText != null)
            crystalText.text = crystalCount.ToString();
    }
}