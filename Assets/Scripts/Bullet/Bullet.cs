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

    // Метод для установки параметров пули (урон, скорость, а также можно добавить цвет и масштаб)
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
        // Предполагается, что "вперёд" - это local right
        _rb.velocity = transform.right * _speed;
        // Уничтожаем снаряд через 3 секунды, если он ни с чем не столкнулся
        Destroy(gameObject, 3f);
    }

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (collision.CompareTag("Enemy"))
    //    {
    //        Debug.Log($"Снаряд попал в {collision.name}. Урон: {_damage}");
    //        collision.GetComponent<Enemy>()?.TakeDamage(_damage);
    //        Destroy(gameObject); // Уничтожение пули
    //    }
    //}

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Bullet") && !collision.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
            Debug.Log($"Пуля уничтожена при столкновении с {collision.gameObject.name}");
        }
    }
}
