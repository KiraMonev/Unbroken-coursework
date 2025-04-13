using System.Collections;
using UnityEngine;

public class MedkitPickup : MonoBehaviour
{
    [Tooltip("Время в секундах до появления ящика после подбора")]
    public float respawnTime = 15f;

    private SpriteRenderer _spriteRenderer;
    private Collider2D _collider;
    private Vector3 _initialPosition;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();
        _initialPosition = transform.position;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //if (other.CompareTag("Player"))
        //{
        //    PlayerHealth playerHP = other.GetComponent<PlayerHealth>();
        //    Debug.Log($"У игрока {playerHP.Health} ХП");
        //    if (playerHP.Health != 0) // Чтобы игрок не смог вылечиться, если умер
        //    {
        //        playerHP.Health += 1;
        //        Debug.Log("Получили 1 ХП");
        //        Debug.Log($"Теперь у игрока {playerHP.Health} ХП");
        //        // Отключаем визуальное отображение и коллайдер
        //        _spriteRenderer.enabled = false;
        //        _collider.enabled = false;
        //        StartCoroutine(Respawn());
        //    }
        //}
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHP = other.GetComponent<PlayerHealth>();
            Debug.Log($"У игрока {playerHP.Health} ХП");
            if (playerHP.Health != 0) // Чтобы игрок не смог вылечиться, если умер
            {
                playerHP.IncreaseHealth(1);
                Debug.Log("Получили 1 ХП");
                Debug.Log($"Теперь у игрока {playerHP.Health} ХП");
                // Отключаем визуальное отображение и коллайдер
                _spriteRenderer.enabled = false;
                _collider.enabled = false;
                StartCoroutine(Respawn());
            }
        }
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnTime);
        transform.position = _initialPosition; // Если позиция могла измениться
        _spriteRenderer.enabled = true;
        _collider.enabled = true;
    }
}
