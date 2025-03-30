using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float _damage;
    private float _speed;
    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // ����� ��� ��������� ���������� ���� (����, ��������, � ����� ����� �������� ���� � �������)
    public void SetParameters(float damage, float speed, Vector3 scale, Color color)
    {
        _damage = damage;
        _speed = speed;
        transform.localScale = scale;
        if (_spriteRenderer != null)
            _spriteRenderer.color = color;
    }

    private void Start()
    {
        // ��������������, ��� "�����" - ��� local right
        _rb.velocity = transform.right * _speed;
        // ���������� ������ ����� 3 �������, ���� �� �� � ��� �� ����������
        Destroy(gameObject, 3f);
    }

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (collision.CompareTag("Enemy"))
    //    {
    //        Debug.Log($"������ ����� � {collision.name}. ����: {_damage}");
    //        collision.GetComponent<Enemy>()?.TakeDamage(_damage);
    //        Destroy(gameObject); // ����������� ����
    //    }
    //}

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Bullet") && !collision.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
            Debug.Log($"���� ���������� ��� ������������ � {collision.gameObject.name}");
        }
    }
}
