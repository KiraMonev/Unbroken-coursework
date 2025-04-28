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

    [Header("��������� �����")]
    [Tooltip("��� ������ ������ (�������, ������, ������ � �.�.)")]
    [SerializeField] private AudioSource playerSource;
    [Tooltip("��� ������� ������")]
    [SerializeField] private AudioSource musicSource;

    [Header("������ ������ ��� Player")]
    [SerializeField] private List<SoundEntry> soundEntries;

    private Dictionary<AudioType, SoundEntry> _soundDict;

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

        // ��������� ������� ��� �������� �������
        _soundDict = new Dictionary<AudioType, SoundEntry>();
        foreach (var entry in soundEntries)
        {
            if (!_soundDict.ContainsKey(entry.type))
                _soundDict[entry.type] = entry;
        }
    }

    /// ������������� ���� ������ �� ������ soundEntries.
    public void PlayPlayer(AudioType type)
    {
        if (_soundDict.TryGetValue(type, out var entry) && entry.clip != null)
        {
            playerSource.PlayOneShot(entry.clip, entry.volume);
        }
    }

    /// ��������� ������� ������. ��� ����� ����� ������ �����������.
    public void PlayMusic(AudioClip musicClip, bool loop = true)
    {
        if (musicSource.clip != musicClip)
        {
            musicSource.clip = musicClip;
            musicSource.loop = loop;
            musicSource.Play();
        }
    }

    /// ���������� ������.
    public void StopMusic()
    {
        musicSource.Stop();
    }
}
