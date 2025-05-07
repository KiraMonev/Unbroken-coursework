using UnityEngine;
using UnityEngine.UI;

public class CrystalManager : MonoBehaviour
{
    public static CrystalManager Instance { get; private set; }

    [Tooltip("Текстовый элемент UI для отображения количества кристаллов")]
    [SerializeField] private Text crystalText;

    private int crystalCount = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start() => UpdateUI();

    public void AddCrystal(int amount)
    {
        crystalCount += amount;
        UpdateUI();
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
            Debug.Log("Недостаточно кристаллов для покупки");
            return false;
        }
    }

    private void UpdateUI()
    {
        if (crystalText != null)
            crystalText.text = crystalCount.ToString();
    }
}