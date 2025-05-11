using UnityEngine;

public class Shop : MonoBehaviour
{
    [SerializeField] private GameObject _shopCanvas;
    public bool isShopping = false;
    public GameObject gameUI;

    private void Awake()
    {
        gameUI = GameObject.FindGameObjectWithTag("GameUI");
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("��� �� ����� � �������");

        if (!other.CompareTag("Player"))
            return;

        OpenShop();
    }

    private void OpenShop()
    {
        Debug.Log("��������� �������");
        Time.timeScale = 0f;
        gameUI.SetActive(false);
        _shopCanvas.SetActive(true);
    }

    public void CloseShop()
    {
        Debug.Log("��������� �������");
        Time.timeScale = 1f;
        gameUI.SetActive(true);
        _shopCanvas.SetActive(false);
    }
}
