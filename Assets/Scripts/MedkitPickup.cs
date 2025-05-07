using System.Collections;
using UnityEngine;

public class MedkitPickup : MonoBehaviour
{
    [Tooltip("����� � �������� �� ��������� ����� ����� �������")]
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
        //    Debug.Log($"� ������ {playerHP.Health} ��");
        //    if (playerHP.Health != 0) // ����� ����� �� ���� ����������, ���� ����
        //    {
        //        playerHP.Health += 1;
        //        Debug.Log("�������� 1 ��");
        //        Debug.Log($"������ � ������ {playerHP.Health} ��");
        //        // ��������� ���������� ����������� � ���������
        //        _spriteRenderer.enabled = false;
        //        _collider.enabled = false;
        //        StartCoroutine(Respawn());
        //    }
        //}
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHP = other.GetComponent<PlayerHealth>();
            Debug.Log($"� ������ {playerHP.Health} ��");
            if (playerHP.Health != 0) // ����� ����� �� ���� ����������, ���� ����
            {
                playerHP.IncreaseHealth(1);
                Debug.Log("�������� 1 ��");
                Debug.Log($"������ � ������ {playerHP.Health} ��");
                // ��������� ���������� ����������� � ���������
                _spriteRenderer.enabled = false;
                _collider.enabled = false;
                StartCoroutine(Respawn());
            }
        }
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnTime);
        transform.position = _initialPosition; // ���� ������� ����� ����������
        _spriteRenderer.enabled = true;
        _collider.enabled = true;
    }
}
