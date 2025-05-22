using UnityEngine;
using System.Collections;

public class AmmoPickup : MonoBehaviour
{
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
        if (!other.CompareTag("Player"))
            return;

        WeaponManager wm = other.GetComponent<WeaponManager>();
        if (wm == null) return;

        WeaponType current = wm.GetCurrentWeaponType();

        wm.RefillAmmo();

        if (current == WeaponType.Pistol
            || current == WeaponType.Uzi
            || current == WeaponType.Rifle
            || current == WeaponType.Shotgun)
        {
            SoundManager.Instance.PlayPickup(PickupSoundType.PickupAmmo);
            _spriteRenderer.enabled = false;
            _collider.enabled = false;
            StartCoroutine(Respawn());
        }  
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnTime);
        transform.position = _initialPosition;
        _spriteRenderer.enabled = true;
        _collider.enabled = true;
    }
}
