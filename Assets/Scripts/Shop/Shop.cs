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

    [Header("Рывок")]
    [SerializeField] private int _dashPrice = 0;
    [SerializeField] private Text _dashPriceText = null;

    [Header("2х урон")]
    [SerializeField] private int _damagePrice = 0;
    [SerializeField] private Text _damagePriceText = null;

    [Header("Броня")]
    [SerializeField] private int _armorPrice = 0;
    [SerializeField] private Text _armorPriceText = null;



    private void Awake()
    {
        gameUI = GameObject.FindGameObjectWithTag("GameUI");
        _crystalManager = CrystalManager.Instance;
        _playerController = FindObjectOfType<PlayerController>();
        _weaponManager = FindObjectOfType<WeaponManager>();
        _playerHealth = FindObjectOfType<PlayerHealth>();

        if (_dashPriceText != null)
            _dashPriceText.text = $"Цена: {_dashPrice}";

        if (_damagePriceText != null)
            _damagePriceText.text = $"Цена: {_damagePrice}";

        if (_armorPriceText != null)
            _armorPriceText.text = $"Цена: {_armorPrice}";
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var uiRoot = GameObject.Find("GameManager")?.transform.Find("UI");
        if (uiRoot != null)
            gameUI = uiRoot.Find("GameStats").gameObject;

        _playerController = FindObjectOfType<PlayerController>();
        _weaponManager = FindObjectOfType<WeaponManager>();
        _playerHealth = FindObjectOfType<PlayerHealth>();
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
        Time.timeScale = 0f;
        isShopping = true;
        gameUI.SetActive(false);
        _shopCanvas.SetActive(true);
    }

    public void CloseShop()
    {
        Time.timeScale = 1f;
        isShopping = false;
        gameUI.SetActive(true);
        _shopCanvas.SetActive(false);

        if (_weaponManager != null)
            _weaponManager.StopAutoFire();
    }

    public void BuyDash()
    {
        if (_crystalManager.SpendCrystal(_dashPrice)) { 
            _playerController.UnlockDash();
            AchievementManager.instance.Unlock("shop");
        }
        else
            return;
    }

    public void BuyDamage()
    {
        if (_crystalManager.SpendCrystal(_damagePrice)) { 
            _weaponManager.UnlockDoubleDamage();
            AchievementManager.instance.Unlock("shop");
        }

        else
            return;
    }

    public void BuyArmor()
    {
        if (!_crystalManager.SpendCrystal(_armorPrice))
        {
            return;
        }

        if (_playerHealth.armor < _playerHealth.maxArmor)
        {
            _playerHealth.IncreaseArmor(1);
            AchievementManager.instance.Unlock("shop");
        }
    }

}
