using UnityEngine;
using System.Collections.Generic;

public enum WeaponSoundType
{
    BallbatAttack,
    KatanaAndKnife,
    Throw,
    UziShoot,
    EmptyAmmo,
    PickupKatanaAndKnife,
    PickupWeapon,
    PistolShoot,
    RifleShoot,
    ShotgunShoot
}

public enum PlayerSoundType
{
    Dash,
    TakeDamage,
    Death
}

public enum EnemiesSoundType
{
    TakeDamage,
    Spotted,
    SpottedMaf,
    Hitted,
    Shot
}

public enum PickupSoundType
{
    PickupCrystal,
    PickupDocument,
    PickupAmmo
}

// ��������� ��� ������ ������
[System.Serializable]
public struct WeaponSoundEntry
{
    public WeaponSoundType type;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume;
}

// ��������� ��� ������ ������
[System.Serializable]
public struct PlayerSoundEntry
{
    public PlayerSoundType type;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume;
}

[System.Serializable]
public struct EnemiesSoundEntry
{
    public EnemiesSoundType type;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume;
}

[System.Serializable]
public struct PickupSoundEntry
{
    public PickupSoundType type;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume;
}


public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("��������� �����")]
    [Tooltip("����� ��� ��������� � ������")]
    [SerializeField] private AudioSource weaponSource;
    [Tooltip("����� �������� ������")]
    [SerializeField] private AudioSource playerSource;
    [Tooltip("������� ������")]
    [SerializeField] private AudioSource musicSource;
    [Tooltip("����� ������")]
    [SerializeField] private AudioSource enemiesSource;
    [Tooltip("����� ������� ���������")]
    [SerializeField] private AudioSource pickupSource;

    [Header("������ ������ ��� ������")]
    [SerializeField] private List<WeaponSoundEntry> weaponSoundEntries;
    [Header("������ ������ ��� ������")]
    [SerializeField] private List<PlayerSoundEntry> playerSoundEntries;
    [Header("������ ������ ��� ������")]
    [SerializeField] private List<EnemiesSoundEntry> enemiesSoundEntries;
    [Header("������ ������ ��� ������� ���������")]
    [SerializeField] private List<PickupSoundEntry> pickupSoundEntries;

    private Dictionary<WeaponSoundType, WeaponSoundEntry> _weaponSounds;
    private Dictionary<PlayerSoundType, PlayerSoundEntry> _playerSounds;
    private Dictionary<EnemiesSoundType, EnemiesSoundEntry> _enemiesSounds;
    private Dictionary<PickupSoundType, PickupSoundEntry> _pickupSounds;

    private void Awake()
    {
        // ��������
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // �������������� ������� ��� �������� �������
        _weaponSounds = new Dictionary<WeaponSoundType, WeaponSoundEntry>();
        foreach (var entry in weaponSoundEntries)
        {
            if (!_weaponSounds.ContainsKey(entry.type))
                _weaponSounds[entry.type] = entry;
        }

        _playerSounds = new Dictionary<PlayerSoundType, PlayerSoundEntry>();
        foreach (var entry in playerSoundEntries)
        {
            if (!_playerSounds.ContainsKey(entry.type))
                _playerSounds[entry.type] = entry;
        }

        _enemiesSounds = new Dictionary<EnemiesSoundType, EnemiesSoundEntry>();
        foreach (var entry in enemiesSoundEntries)
        {
            if (!_enemiesSounds.ContainsKey(entry.type))
                _enemiesSounds[entry.type] = entry;
        }

        _pickupSounds = new Dictionary<PickupSoundType, PickupSoundEntry>();
        foreach (var entry in pickupSoundEntries)
        {
            if (!_pickupSounds.ContainsKey(entry.type))
                _pickupSounds[entry.type] = entry;
        }
    }

    // ������������� ����, ��������� � �������
    public void PlayWeapon(WeaponSoundType type)
    {
        if (_weaponSounds.TryGetValue(type, out var entry) && entry.clip != null)
        {
            weaponSource.PlayOneShot(entry.clip, entry.volume);
        }
    }

    // ������������� ����, ��������� � ���������� ������
    public void PlayPlayer(PlayerSoundType type)
    {
        if (_playerSounds.TryGetValue(type, out var entry) && entry.clip != null)
        {
            playerSource.PlayOneShot(entry.clip, entry.volume);
        }
    }

    public void PlayEnemies(EnemiesSoundType type)
    {
        if (_enemiesSounds.TryGetValue(type, out var entry) && entry.clip != null)
        {
            enemiesSource.PlayOneShot(entry.clip, entry.volume);
        }
    }

    public void PlayPickup(PickupSoundType type)
    {
        if (_pickupSounds.TryGetValue(type, out var entry) && entry.clip != null)
        {
            pickupSource.PlayOneShot(entry.clip, entry.volume);
        }
    }

    // ��������� ������� ������. ��� ����� ����� ������ �����������
    public void PlayMusic(AudioClip musicClip, bool loop = true)
    {
        if (musicSource.clip != musicClip)
        {
            musicSource.clip = musicClip;
            musicSource.loop = loop;
            musicSource.Play();
        }
    }

    // ���������� ��� ������
    public void StopMusic()
    {
        musicSource.Stop();
    }
}
