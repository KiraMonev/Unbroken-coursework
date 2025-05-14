using UnityEngine;
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

    [Header("2х Урон")]
    [SerializeField] private int _damagePrice = 0;
    [SerializeField] private Text _damagePriceText = null;

    [Header("Броня")]
    [SerializeField] private int _armorPrice = 0;
    [SerializeField] private Text _armorPriceText = null;



    private void Awake()
    {
        gameUI = GameObject.FindGameObjectWithTag("GameUI");
        _crystalManager = FindObjectOfType<CrystalManager>();
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


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        OpenShop();
    }

    private void OpenShop()
    {
        Debug.Log("Открываем магазин");
        Time.timeScale = 0f;
        gameUI.SetActive(false);
        _shopCanvas.SetActive(true);
    }

    public void CloseShop()
    {
        Debug.Log("Закрываем магазин");
        Time.timeScale = 1f;
        gameUI.SetActive(true);
        _shopCanvas.SetActive(false);
    }

    public void BuyDash()
    {
        Debug.Log("Пытаемся купить рывок");
        if (_crystalManager.SpendCrystal(_dashPrice))
            _playerController.UnlockDash();
        else
            return;
    }

    public void BuyDamage()
    {
        Debug.Log("Пытаемся купить двойной урон");
        if (_crystalManager.SpendCrystal(_damagePrice))
            _weaponManager.UnlockDoubleDamage();
        else
            return;
    }

    public void BuyArmor()
    {
        Debug.Log("Пытаемся купить броню 1шт.");
        if (_crystalManager.SpendCrystal(_armorPrice) && _playerHealth.armor < _playerHealth.maxArmor)
            _playerHealth.IncreaseArmor(1);
        else
            return;
    }
}
