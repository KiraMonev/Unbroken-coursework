using UnityEngine;
using System.Collections;

public class WeaponPickup : MonoBehaviour
{
    [SerializeField] private WeaponType _weaponType;
    public WeaponType WeaponType => _weaponType;

    [SerializeField] private Rigidbody2D _rigidBody;
    [SerializeField] private Collider2D _collider;
    [SerializeField] private float _enableTriggerDelay = 0.5f;

    private void Awake()
    {
        if (_rigidBody == null)
            _rigidBody = GetComponent<Rigidbody2D>();
        if (_collider == null)
            _collider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        // �������� �������� ��������� ������ ����� ��������
        var playerController = PlayerController.Instance;
        if (playerController != null)
        {
            Collider2D playerCollider = playerController.GetComponent<Collider2D>();
            if (playerCollider != null)
            {
                // ��������� ������������ �� �����, ����� �� ������������ ������ ����� ����� ������
                Physics2D.IgnoreCollision(_collider, playerCollider, true);
                StartCoroutine(ReenableCollisionAfterDelay(playerCollider));
            }
            else
            {
                Debug.LogWarning("WeaponPickup: � ������ ��� Collider2D!");
            }
        }
        else
        {
            Debug.LogWarning("WeaponPickup: PlayerController.Instance == null!");
        }
    }

    private IEnumerator ReenableCollisionAfterDelay(Collider2D playerCollider)
    {
        yield return new WaitForSeconds(_enableTriggerDelay);
        // ��� ��� ���������, ��� ���������� ����
        if (_collider != null && playerCollider != null)
        {
            Physics2D.IgnoreCollision(_collider, playerCollider, false);
        }
    }

    public void Throw(Vector2 direction, float force)
    {
        if (_rigidBody == null || _collider == null)
            return;

        _collider.isTrigger = false;
        _rigidBody.isKinematic = false;
        _rigidBody.velocity = direction * force;
        StartCoroutine(EnableTriggerAfterDelay());
    }

    private IEnumerator EnableTriggerAfterDelay()
    {
        yield return new WaitForSeconds(_enableTriggerDelay);
        if (_collider != null)
            _collider.isTrigger = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // ��� ������ ������ ��������� �� ���-���� � ������������� � ��������� � �������
        if (_rigidBody != null)
        {
            _rigidBody.velocity = Vector2.zero;
            _rigidBody.angularVelocity = 0f;
        }
        if (_collider != null)
            _collider.isTrigger = true;
    }
}
