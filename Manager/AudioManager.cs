using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 音频管理器 —— 管理 BGM 和 SFX 的播放。
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("背景音乐")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private float bgmVolume = 0.5f;
    [SerializeField] private bool playBgmOnStart = true;

    [Header("音效")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private float sfxVolume = 1f;

    [Header("BGM 播放列表")]
    [SerializeField] private List<AudioClip> bgmClips;

    private int currentBgmIndex = -1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (playBgmOnStart && bgmClips.Count > 0)
        {
            PlayBgm(0);
        }
    }

    private void InitAudioSources()
    {
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
        }
        bgmSource.volume = bgmVolume;

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }
        sfxSource.volume = sfxVolume;
    }

    // ==================== BGM ====================

    /// <summary>播放指定索引的 BGM。</summary>
    public void PlayBgm(int index)
    {
        if (index < 0 || index >= bgmClips.Count)
        {
            Debug.LogWarning($"[AudioManager] BGM 索引 {index} 无效");
            return;
        }

        if (currentBgmIndex == index && bgmSource.isPlaying) return;

        currentBgmIndex = index;
        bgmSource.clip = bgmClips[index];
        bgmSource.Play();
    }

    /// <summary>暂停 BGM。</summary>
    public void PauseBgm() => bgmSource.Pause();

    /// <summary>恢复 BGM。</summary>
    public void ResumeBgm() => bgmSource.UnPause();

    /// <summary>停止 BGM。</summary>
    public void StopBgm()
    {
        bgmSource.Stop();
        currentBgmIndex = -1;
    }

    /// <summary>设置 BGM 音量（0~1）。</summary>
    public void SetBgmVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        bgmSource.volume = bgmVolume;
    }

    // ==================== SFX ====================

    /// <summary>播放一个音效。</summary>
    public void PlaySfx(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume * volumeScale);
    }

    /// <summary>从列表中随机播放一个音效。</summary>
    public void PlayRandomSfx(List<AudioClip> clips, float volumeScale = 1f)
    {
        if (clips == null || clips.Count == 0) return;
        PlaySfx(clips[Random.Range(0, clips.Count)], volumeScale);
    }

    /// <summary>设置 SFX 音量（0~1）。</summary>
    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
    }

    // ==================== 总控 ====================

    /// <summary>静音/取消静音所有音频。</summary>
    public void SetMute(bool mute)
    {
        bgmSource.mute = mute;
        sfxSource.mute = mute;
    }
}
