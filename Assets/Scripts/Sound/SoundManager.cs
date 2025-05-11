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

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("��������� �����")]
    [Tooltip("����� ��� ��������� � ������")]
    [SerializeField] private AudioSource weaponSource;
    [Tooltip("����� �������� ������ (�����, ����, ������)")]
    [SerializeField] private AudioSource playerSource;
    [Tooltip("������� ������")]
    [SerializeField] private AudioSource musicSource;

    [Header("������ ������ ��� ������")]
    [SerializeField] private List<WeaponSoundEntry> weaponSoundEntries;
    [Header("������ ������ ��� ������")]
    [SerializeField] private List<PlayerSoundEntry> playerSoundEntries;

    private Dictionary<WeaponSoundType, WeaponSoundEntry> _weaponSounds;
    private Dictionary<PlayerSoundType, PlayerSoundEntry> _playerSounds;

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
