using UnityEngine;
using UnityEngine.UI;

public class ManagerLevel1 : MonoBehaviour
{
    [Header("��������� ������")]
    [Tooltip("������� ����� ������� ���� �� ������")]
    public int totalDigits = 5;

    private int collectedDigits = 0;
    [SerializeField] private Text _digitsText;
    private bool isPistolUsed = false;
    public bool PistolUsed => isPistolUsed;
    public int CollectedDigits => collectedDigits;

    [SerializeField] private int addScoreForPistol = 50;


    public void RegisterPistolShot()
    {
        if (!isPistolUsed)
        {
            isPistolUsed = true;
            Debug.Log("[ManagerLevel1] �������� ��� �����������! ������� ����� ��������.");
        }
    }

    public void RegisterDigitCollected()
    {
        collectedDigits++;
        UpdateUI();
        Debug.Log($"[ManagerLevel1] ������� ����: {collectedDigits}/{totalDigits}");
    }

    public bool CanProceed()
    {
        if (collectedDigits < totalDigits)
            return false;

        int additionalLevelScore = isPistolUsed ? 0 : addScoreForPistol;
        ScoreManager.Instance.AddScore(additionalLevelScore);
        return true;
    }

    private void UpdateUI()
    {
        if (_digitsText != null)
            _digitsText.text = CollectedDigits.ToString();
    }
}
