using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자: 이예린
/// 
/// 게임 내 사운드 관련 기능 관리하는 매니저
/// </summary>
public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;

    [SerializeField] AudioSource bgmSource;
    [SerializeField] AudioSource sfxSource;

    [Tooltip("첫번째 : 타이틀\n두번째 : 게임 배경음\n세번째 : 게임 종료")]
    [SerializeField] List<AudioClip> bgmClipList = new();

    public float BGMVolme { get { return bgmSource.volume; } set { bgmSource.volume = value; } }
    public float SFXVolme { get { return sfxSource.volume; } set { sfxSource.volume = value; } }

    #region Unity Event
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            GameModeManager.SoundManager = instance;
            DontDestroyOnLoad(this);
        }
        else
            Destroy(gameObject);
    }
    #endregion

    #region BGM
    public void PlayBGM(bgmClip bgm)
    {
        if (bgmClipList.Count == 0)
            return;

        if (bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }
        bgmSource.clip = bgmClipList[(int)bgm];
        bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource.isPlaying == false)
            return;

        bgmSource.Stop();
    }
    #endregion

    #region SFX
    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    public void StopSFX()
    {
        if (sfxSource.isPlaying == false)
            return;

        sfxSource.Stop();
    }
    #endregion
}

/// <summary>
/// 게임 내 BGM 종류를 구분하기 위한 열거형
/// </summary>
public enum bgmClip
{
    title,
    game,
    // gameEnd
}