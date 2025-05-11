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

    [Header("Рывок")]
    [SerializeField] private int _dashPrice;
    [SerializeField] private Text _dashPriceText;

    [Header("2х Урон")]
    [SerializeField] private int _damagePrice;
    [SerializeField] private Text _damagePriceText;



    private void Awake()
    {
        gameUI = GameObject.FindGameObjectWithTag("GameUI");
        _crystalManager = FindObjectOfType<CrystalManager>();
        _playerController = FindObjectOfType<PlayerController>();
        _weaponManager = FindObjectOfType<WeaponManager>();
        _dashPriceText.text = "Цена: " + _dashPrice.ToString();
        _damagePriceText.text = "Цена: " + _damagePrice.ToString();
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Кто то зашел в магазин");

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
        if (_crystalManager.SpendCrystal(_dashPrice))
            _playerController.UnlockDash();
        else
            return;
    }

    public void BuyDamage()
    {
        if (_crystalManager.SpendCrystal(_damagePrice))
            _weaponManager.UnlockDoubleDamage();
        else
            return;
    }
}
