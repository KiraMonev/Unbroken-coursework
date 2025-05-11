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

// Структура для звуков оружия
[System.Serializable]
public struct WeaponSoundEntry
{
    public WeaponSoundType type;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume;
}

// Структура для звуков игрока
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

    [Header("Источники звука")]
    [Tooltip("Звуки при выстрелах и атаках")]
    [SerializeField] private AudioSource weaponSource;
    [Tooltip("Звуки действий игрока (рывок, урон, смерть)")]
    [SerializeField] private AudioSource playerSource;
    [Tooltip("Фоновая музыка")]
    [SerializeField] private AudioSource musicSource;

    [Header("Список звуков для оружия")]
    [SerializeField] private List<WeaponSoundEntry> weaponSoundEntries;
    [Header("Список звуков для игрока")]
    [SerializeField] private List<PlayerSoundEntry> playerSoundEntries;

    private Dictionary<WeaponSoundType, WeaponSoundEntry> _weaponSounds;
    private Dictionary<PlayerSoundType, PlayerSoundEntry> _playerSounds;

    private void Awake()
    {
        // Синглтон
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

        // Инициализируем словари для быстрого доступа
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

    // Воспроизвести звук, связанный с оружием
    public void PlayWeapon(WeaponSoundType type)
    {
        if (_weaponSounds.TryGetValue(type, out var entry) && entry.clip != null)
        {
            weaponSource.PlayOneShot(entry.clip, entry.volume);
        }
    }

    // Воспроизвести звук, связанный с действиями игрока
    public void PlayPlayer(PlayerSoundType type)
    {
        if (_playerSounds.TryGetValue(type, out var entry) && entry.clip != null)
        {
            playerSource.PlayOneShot(entry.clip, entry.volume);
        }
    }

    // Запустить фоновую музыку. При новом клипе старый остановится
    public void PlayMusic(AudioClip musicClip, bool loop = true)
    {
        if (musicSource.clip != musicClip)
        {
            musicSource.clip = musicClip;
            musicSource.loop = loop;
            musicSource.Play();
        }
    }

    // Остановить фон музыку
    public void StopMusic()
    {
        musicSource.Stop();
    }
}
