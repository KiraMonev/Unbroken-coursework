using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct SoundEntry
{
    public AudioType type;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume;
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Источники звука")]
    [Tooltip("Для звуков игрока (выстрел, подбор, бросок и т.д.)")]
    [SerializeField] private AudioSource playerSource;
    [Tooltip("Для фоновой музыки")]
    [SerializeField] private AudioSource musicSource;

    [Header("Список звуков для Player")]
    [SerializeField] private List<SoundEntry> soundEntries;

    private Dictionary<AudioType, SoundEntry> _soundDict;

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

        // Заполняем словарь для быстрого доступа
        _soundDict = new Dictionary<AudioType, SoundEntry>();
        foreach (var entry in soundEntries)
        {
            if (!_soundDict.ContainsKey(entry.type))
                _soundDict[entry.type] = entry;
        }
    }

    /// Воспроизвести звук игрока из списка soundEntries.
    public void PlayPlayer(AudioType type)
    {
        if (_soundDict.TryGetValue(type, out var entry) && entry.clip != null)
        {
            playerSource.PlayOneShot(entry.clip, entry.volume);
        }
    }

    /// Запустить фоновую музыку. При новом клипе старый остановится.
    public void PlayMusic(AudioClip musicClip, bool loop = true)
    {
        if (musicSource.clip != musicClip)
        {
            musicSource.clip = musicClip;
            musicSource.loop = loop;
            musicSource.Play();
        }
    }

    /// Остановить музыку.
    public void StopMusic()
    {
        musicSource.Stop();
    }
}
