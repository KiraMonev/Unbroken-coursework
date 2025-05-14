using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    [SerializeField] private GameObject _shopCanvas;
    public bool isShopping = false;
    public GameObject gameUI;
    private CrystalManager _crystalManager;
    private PlayerController _playerController;
    private WeaponManager _weaponManager;
    private PlayerHealth _playerHealth;

    [Header("–ывок")]
    [SerializeField] private int _dashPrice = 0;
    [SerializeField] private Text _dashPriceText = null;

    [Header("2х ”рон")]
    [SerializeField] private int _damagePrice = 0;
    [SerializeField] private Text _damagePriceText = null;

    [Header("Ѕрон€")]
    [SerializeField] private int _armorPrice = 0;
    [SerializeField] private Text _armorPriceText = null;



    private void Awake()
    {
        gameUI = GameObject.FindGameObjectWithTag("GameUI");
        _crystalManager = CrystalManager.Instance;
        _playerController = FindObjectOfType<PlayerController>();
        _weaponManager = FindObjectOfType<WeaponManager>();
        _playerHealth = FindObjectOfType<PlayerHealth>();

        Debug.Log($"Shop: playerController = {_playerController}, weaponManager = {_weaponManager}, playerHealth = {_playerHealth}");


        if (_dashPriceText != null)
            _dashPriceText.text = $"÷ена: {_dashPrice}";

        if (_damagePriceText != null)
            _damagePriceText.text = $"÷ена: {_damagePrice}";

        if (_armorPriceText != null)
            _armorPriceText.text = $"÷ена: {_armorPrice}";
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var uiRoot = GameObject.Find("GameManager")?.transform.Find("UI");
        if (uiRoot != null)
            gameUI = uiRoot.Find("GameStats").gameObject;

        _playerController = FindObjectOfType<PlayerController>();
        _weaponManager = FindObjectOfType<WeaponManager>();
        _playerHealth = FindObjectOfType<PlayerHealth>();

        Debug.Log($"Shop: playerController = {_playerController}, weaponManager = {_weaponManager}, playerHealth = {_playerHealth}");

    }


    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        OpenShop();
    }

    private void OpenShop()
    {
        Debug.Log("ќткрываем магазин");
        Time.timeScale = 0f;
        gameUI.SetActive(false);
        _shopCanvas.SetActive(true);
    }

    public void CloseShop()
    {
        Debug.Log("«акрываем магазин");
        Time.timeScale = 1f;
        gameUI.SetActive(true);
        _shopCanvas.SetActive(false);
    }

    public void BuyDash()
    {
        Debug.Log("ѕытаемс€ купить рывок");
        if (_crystalManager.SpendCrystal(_dashPrice))
            _playerController.UnlockDash();
        else
            return;
    }

    public void BuyDamage()
    {
        Debug.Log("ѕытаемс€ купить двойной урон");
        if (_crystalManager.SpendCrystal(_damagePrice))
            _weaponManager.UnlockDoubleDamage();
        else
            return;
    }

    public void BuyArmor()
    {
        Debug.Log("ѕытаемс€ купить бронюЕ");

        if (!_crystalManager.SpendCrystal(_armorPrice))
        {
            Debug.Log("Ц недостаточно кристаллов");
            return;
        }

        if (_playerHealth.armor < _playerHealth.maxArmor)
        {
            _playerHealth.IncreaseArmor(1);
            Debug.Log($"Ц брон€ добавлена, сейчас: {_playerHealth.armor}");
        }
        else
        {
            Debug.Log("Ц брон€ уже полна€, покупка отменена");
        }
    }

}
