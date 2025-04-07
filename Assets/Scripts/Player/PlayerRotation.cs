using UnityEngine;

public class PlayerRotation : MonoBehaviour
{
    [SerializeField] private Camera _mainCamera;
    private PlayerHealth _playerHealth;

    private void Awake()
    {
        _playerHealth = GetComponent<PlayerHealth>();
    } 
    private void Update()
    {
        if (_playerHealth.isDead) return;
        RotateTowardMouse();
    }

    private void RotateTowardMouse()
    {
        Vector3 mousePosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = transform.position.z;

        Vector2 direction = mousePosition - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
