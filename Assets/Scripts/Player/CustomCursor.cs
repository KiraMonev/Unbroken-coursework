using UnityEngine;
using UnityEngine.SceneManagement;
public class CustomCursor : MonoBehaviour
{
    public static CustomCursor Instance { get; private set; }

    [SerializeField] private Texture2D _cursorTexture;
    [SerializeField] private Vector2 _cursorHotSpot = Vector2.zero;

    private void Awake()
    {
        // ≈сли это первый экземпл€р Ч сохран€ем и инициализируем
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    private void Start()
    {
        Cursor.SetCursor(_cursorTexture, _cursorHotSpot, CursorMode.Auto);
    }
}