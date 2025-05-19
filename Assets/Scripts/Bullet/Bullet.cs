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
        _rb.velocity = transform.right * _speed;
        Destroy(gameObject, 3f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var other = collision.gameObject;

        if (other.CompareTag("Enemy"))
        {
            var mafiaEnemy = other.GetComponent<Mafia>();
            if (mafiaEnemy != null)
            {
                int dmg = Mathf.RoundToInt(_damage);
                mafiaEnemy.TakeDamage(dmg);
                Debug.Log($"Пуля нанесла урон: {dmg}");
            }

            Destroy(gameObject);
            return;
        }

        if (!other.CompareTag("Bullet") && !other.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}
