using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// 개발자: 이예린
/// 
/// 게임 내 사운드 관련 기능 관리하는 매니저
/// </summary>
public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;

    [Header("Audio Sources")]
    [SerializeField] AudioSource bgmSource;
    [SerializeField] AudioSource sfxSource;

    [Header("Audio Mixer")]
    [SerializeField] AudioMixer audioMixer;
    private AudioMixerGroup sfxGroup;

    [Header("Audio Clip List")]
    [Tooltip("첫번째 : 타이틀\n두번째 : 게임 배경음\n세번째 : 게임 종료")]
    [SerializeField] List<AudioClip> bgmClipList = new();
    // TODO... 이후 데이터 테이블을 읽어와 스크립터블 오브젝트로 받을 예정
    [SerializeField] List<int> sfxIDList = new();
    [SerializeField] List<AudioClip> sfxClipList = new();

    private Dictionary<int, AudioClip> sfxClipDic = new();
    private bool isSFXReady;

    public float BGMVolume { get { return bgmSource.volume; } set { bgmSource.volume = value; } }
    /// <summary>
    /// 전체 SFX 볼륨 (0~1) - AudioMixer 기반
    /// </summary>
    public float SFXVolume
    {
        get
        {
            if (audioMixer.GetFloat("SFXVolume", out float dB))
            {
                // dB → 0~1 범위로 변환
                return Mathf.Pow(10f, dB / 20f);
            }
            return 1f;
        }
        set
        {
            // 0~1 → dB 변환 (0 → -80dB, 1 → 0dB)
            float dB = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f;
            audioMixer.SetFloat("SFXVolume", dB);
        }
    }

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

        // "SFXGroup"이라는 이름의 오디오 믹서 그룹을 찾아 첫 번째 그룹을 가져옴
        sfxGroup = audioMixer.FindMatchingGroups("SFXGroup")[0];

        // AudioSource에 그룹 할당
        sfxSource.outputAudioMixerGroup = sfxGroup;
    }

    private void Start()
    {
        if (sfxClipList.Count != sfxIDList.Count) return;
        isSFXReady = false;

        // 각 SFX ID와 오디오 클립을 딕셔너리에 매핑
        foreach (int id in sfxIDList)
        {
            foreach (AudioClip clip in sfxClipList)
            {
                sfxClipDic.Add(id, clip);
            }
        }

        isSFXReady = true;
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
    /// <summary>
    /// 지정된 ID의 SFX를 재생하는 메서드
    /// 플레이어 동작이나 UI 입력 등, 위치와 무관한 2D 사운드에 사용됨
    /// </summary>
    /// <param name="id">재생할 SFX의 고유 ID</param>
    /// <returns>
    /// PlayResult.NotReady : SFX 시스템이 아직 준비되지 않음  
    /// PlayResult.Success  : 정상적으로 재생됨  
    /// PlayResult.NotFound : 해당 ID의 SFX가 존재하지 않음
    /// </returns>
    public PlaySFXResult PlaySFX(int id)
    {
        if (!isSFXReady) return PlaySFXResult.NotReady;

        if (sfxClipDic.TryGetValue(id, out var clip))
        {
            sfxSource.PlayOneShot(clip);
            return PlaySFXResult.Success;
        }
        return PlaySFXResult.NotFound;
    }

    /// <summary>
    /// 지정된 ID의 SFX를 재생하는 메서드
    /// 몬스터, 환경 등 위치 기반의 3D 사운드 재생에 사용됨
    /// </summary>
    /// <param name="id">재생할 SFX의 고유 ID</param>
    /// <param name="source">사운드를 출력할 AudioSource (3D 공간에 배치된 소스)</param>
    /// <returns>
    /// PlayResult.NotReady : SFX 시스템이 아직 준비되지 않음  
    /// PlayResult.Success  : 정상적으로 재생됨  
    /// PlayResult.NotFound : 해당 ID의 SFX가 존재하지 않음
    /// </returns>
    public PlaySFXResult PlaySFX(int id, AudioSource source)
    {
        if (!isSFXReady) return PlaySFXResult.NotReady;

        if (sfxClipDic.TryGetValue(id, out var clip))
        {
            // AudioSource가 아직 그룹에 연결되지 않았다면 연결
            if (source.outputAudioMixerGroup != sfxGroup)
                source.outputAudioMixerGroup = sfxGroup;

            source.PlayOneShot(clip);
            return PlaySFXResult.Success;
        }
        return PlaySFXResult.NotFound;
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

/// <summary>
/// SFX 재생 결과를 구분하기 위한 열거형
/// </summary>
public enum PlaySFXResult 
{ 
    NotReady, // SFX 시스템(딕셔너리)가 준비되지 않음
    Success, 
    NotFound 
}