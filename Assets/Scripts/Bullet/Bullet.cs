using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float _damage;
    private float _speed;
    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    // ����� ��� ��������� ���������� ������� (����, ��������)
    public void SetParameters(float damage, float speed)
    {
        _damage = damage;
        _speed = speed;
    }

    private void Start()
    {
        // ��������������, ��� "�����" - ��� local right
        _rb.velocity = transform.right * _speed;
        // ���������� ������ ����� 3 �������, ���� �� �� � ��� �� ����������
        Destroy(gameObject, 3f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // ����� ����� ��������� ��� ����� � ������� ����
        Debug.Log($"������ ����� � {collision.name}. ����: {_damage}");
        // ���������� ������ ��� ������������
        Destroy(gameObject);
    }
}
