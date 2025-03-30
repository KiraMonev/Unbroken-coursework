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

    // Метод для установки параметров снаряда (урон, скорость)
    public void SetParameters(float damage, float speed)
    {
        _damage = damage;
        _speed = speed;
    }

    private void Start()
    {
        // Предполагается, что "вперёд" - это local right
        _rb.velocity = transform.right * _speed;
        // Уничтожаем снаряд через 3 секунды, если он ни с чем не столкнулся
        Destroy(gameObject, 3f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Здесь можно проверить тег врага и нанести урон
        Debug.Log($"Снаряд попал в {collision.name}. Урон: {_damage}");
        // Уничтожаем снаряд при столкновении
        Destroy(gameObject);
    }
}
