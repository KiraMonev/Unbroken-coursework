using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int health;
    [SerializeField] private int numOfHearts;

    [Header("Armor Settings")]
    [Tooltip("������� ���������� ������ �����")]
    [SerializeField] public int armor;
    [Tooltip("�������� �����, ������� ����� ������")]
    [SerializeField] public int maxArmor = 1;

    [Header("UI Settings")]
    [Tooltip("Tag for heart UI images in the scene.")]
    [SerializeField] private string heartTag = "HeartImage";
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite emptyHeart;

    [SerializeField] private string armorTag = "ArmorImage";
    [SerializeField] private Sprite fullArmorIcon;
    [SerializeField] private Sprite emptyArmorIcon;

    private Image[] hearts;
    private Image[] armors;

    public bool isDead = false;
    private SpriteRenderer _spr;
    private Color _originalColor;
    private DeathMenu _deathMenu;

    public int Health
    {
        get => health;
        set => health = Mathf.Clamp(value, 0, numOfHearts);
    }
    public int Armor
    {
        get => armor;
        set => armor = Mathf.Clamp(value, 0, maxArmor);
    }
    private void Awake()
    {
        _spr = GetComponent<SpriteRenderer>();
        _originalColor = _spr.color;
        _deathMenu = FindObjectOfType<DeathMenu>();
        SceneManager.sceneLoaded += OnSceneLoaded;
        FindHearts();
        FindArmors();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(FindUIAfterDelay());
    }

    private IEnumerator FindUIAfterDelay()
    {
        yield return null;
        FindHearts();
        FindArmors();
    }

    private void FindHearts()
    {
        var heartObjects = GameObject.FindGameObjectsWithTag(heartTag);
        Debug.Log($"Found {heartObjects.Length} heart objects");
        hearts = heartObjects
            .OrderBy(go => go.name)
            .Select(go => go.GetComponent<Image>())
            .Where(img => img != null)
            .ToArray();
        Debug.Log($"Hearts array length: {hearts.Length}");
    }

    private void FindArmors()
    {
        var armorObjects = GameObject.FindGameObjectsWithTag(armorTag);
        armors = armorObjects.OrderBy(go => go.name)
            .Select(go => go.GetComponent<Image>())
            .Where(img => img != null)
            .ToArray();
    }
    private void FixedUpdate()
    {
        if (hearts == null || hearts.Length == 0)
            return;

        health = Mathf.Min(health, numOfHearts);
        Armor = Mathf.Min(Armor, maxArmor);

        for (int i = 0; i < hearts.Length; i++)
        {
            Image img = hearts[i];
            if (img == null) continue;

            img.sprite = (i < health) ? fullHeart : emptyHeart;
            img.enabled = (i < numOfHearts);
        }
        for (int i = 0; i < armors.Length; i++)
        {
            if (armors[i] == null) continue;
            armors[i].sprite = (i < Armor) ? fullArmorIcon : emptyArmorIcon;
            armors[i].enabled = (i < maxArmor);
        }
    }

    private IEnumerator FlashRed()
    {
        _spr.color = new Color32(255, 105, 105, 255);
        yield return new WaitForSeconds(0.15f);
        _spr.color = _originalColor;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        SoundManager.Instance.PlayPlayer(PlayerSoundType.TakeDamage);
        StartCoroutine(FlashRed());

        // ������� ��������� �����
        int remainingDamage = damage;
        if (Armor > 0)
        {
            int absorb = Mathf.Min(Armor, damage);
            Armor -= absorb;
            remainingDamage -= absorb;

        }

        // ���� ����� �� ��������� �������� ����
        if (remainingDamage > 0)
        {
            Health -= remainingDamage;

            if (Health <= 0)
            {
                isDead = true;
                SoundManager.Instance.PlayPlayer(PlayerSoundType.Death);
                float playTime = Time.timeSinceLevelLoad;
                int kills = GameAnalytics.Instance.GetCurrentKills();

                // Сохраняем данные
                GameAnalytics.Instance.SaveSessionData(playTime);

                // Показываем экран смерти
                DeathScreenUI.Instance.ShowDeathScreen(playTime, kills);

                _deathMenu.ShowDeathMenu();
            }
        }
    }

    public void IncreaseHealth(int amount)
    {
        Health += amount;
    }

    public void IncreaseArmor(int amount)
    {
        Armor += amount;
    }

    public void SetFullHealth()
    {
        isDead = false;
        Health = numOfHearts;
    }
}