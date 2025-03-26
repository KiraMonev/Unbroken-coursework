using UnityEngine;
public class CustomCursor : MonoBehaviour
{
    [SerializeField] private Texture2D _cursorTexture;
    [SerializeField] private Vector2 _cursorHotSpot = Vector2.zero;
    private void Start()
    {
        Cursor.SetCursor(_cursorTexture, _cursorHotSpot, CursorMode.Auto);
    }
}