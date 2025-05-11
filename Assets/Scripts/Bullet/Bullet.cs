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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var other = collision.gameObject;

        // Если вражеский объект — наносим ему урон
        if (other.CompareTag("Enemy"))
        {
            var mafiaEnemy = other.GetComponent<Mafia>();
            if (mafiaEnemy != null)
            {
                int dmg = Mathf.RoundToInt(_damage);
                mafiaEnemy.TakeDamage(dmg);
                Debug.Log($"Damage of bullet = {dmg}");
            }

            Destroy(gameObject);
            return;
        }

        // Если столкновение ни с пулей, ни с игроком — просто уничтожаем пулю
        if (!other.CompareTag("Bullet") && !other.CompareTag("Player"))
        {
            Destroy(gameObject);
            Debug.Log($"Пуля уничтожена при столкновении с {other.name}");
        }
    }
}
