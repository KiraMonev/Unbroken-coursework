using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int health;
    [SerializeField] private int numOfHearts;
    [SerializeField] private Image[] hearts;
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite emptyHeart;

    public bool isDead = false;
    private SpriteRenderer _spr;
    private Color _originalColor;

    public int Health
    {
        get { return health; }  
        set { health = Mathf.Clamp(value, 0, numOfHearts); } // Устанавливаем здоровье с ограничениями
    }

    private void Awake()
    {
        _spr = GetComponent<SpriteRenderer>();
        _originalColor = _spr.color;
    }

    private void FixedUpdate()
    {
        if (health > numOfHearts)
        {
            health = numOfHearts;
        }
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < health)
                hearts[i].sprite = fullHeart;
            else
                hearts[i].sprite = emptyHeart;

            if (i < numOfHearts)
                hearts[i].enabled = true;
            else
                hearts[i].enabled = false;
        }
    }

    private IEnumerator FlashRed()
    {
        _spr.color = new Color32(255, 105, 105, 255);
        yield return new WaitForSeconds(0.15f);
        _spr.color = _originalColor;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        Health -= damage;
        StartCoroutine(FlashRed());
        //audioManager.PlayTakeDamageSound(); нету
        if (Health <= 0)
        {
            isDead = true;
            //anim.SetTrigger("Die"); наверное не будет
            //deathPanel.SetActive(true); нету
        }
    }

    public void IncreaseHealth(int healthAmount)
    {
        Health += 1;
    }
}
