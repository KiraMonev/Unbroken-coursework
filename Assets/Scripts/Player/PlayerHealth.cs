using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int health;
    [SerializeField] private int numOfHearts;

    [Header("UI Settings")]
    [Tooltip("Tag for heart UI images in the scene.")]
    [SerializeField] private string heartTag = "HeartImage";
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite emptyHeart;

    private Image[] hearts;

    public bool isDead = false;
    private SpriteRenderer _spr;
    private Color _originalColor;

    public int Health
    {
        get => health;
        set => health = Mathf.Clamp(value, 0, numOfHearts);
    }

    private void Awake()
    {
        _spr = GetComponent<SpriteRenderer>();
        _originalColor = _spr.color;
        SceneManager.sceneLoaded += OnSceneLoaded;
        FindHearts();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(FindHeartsAfterDelay());
    }

    private IEnumerator FindHeartsAfterDelay()
    {
        yield return null;
        FindHearts();
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

    private void FixedUpdate()
    {
        if (hearts == null || hearts.Length == 0)
            return;

        health = Mathf.Min(health, numOfHearts);

        for (int i = 0; i < hearts.Length; i++)
        {
            Image img = hearts[i];
            if (img == null) continue;

            img.sprite = (i < health) ? fullHeart : emptyHeart;
            img.enabled = (i < numOfHearts);
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

        Health -= damage;
        StartCoroutine(FlashRed());

        if (Health <= 0)
        {
            isDead = true;
        }
    }

    public void IncreaseHealth(int amount)
    {
        Health += amount;
    }
}