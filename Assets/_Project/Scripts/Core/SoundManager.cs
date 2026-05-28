using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource seSource;

    private void Awake()
    {
        // シングルトンの設定
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// BGMを再生します。
    /// </summary>
    /// <param name="clip">再生するBGMのAudioClip</param>
    /// <param name="loop">ループ再生するかどうか</param>
    public void PlayBGM(AudioClip clip, bool loop = true)
    {
        if (bgmSource == null || clip == null) return;

        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.Play();
    }

    /// <summary>
    /// BGMを停止します。
    /// </summary>
    public void StopBGM()
    {
        if (bgmSource == null) return;
        bgmSource.Stop();
    }

    /// <summary>
    /// SE（効果音）を再生します。
    /// </summary>
    /// <param name="clip">再生するSEのAudioClip</param>
    public void PlaySE(AudioClip clip)
    {
        if (seSource == null || clip == null) return;

        // PlayOneShotを使うことで、複数のSEが重なっても再生される
        seSource.PlayOneShot(clip);
    }
}
