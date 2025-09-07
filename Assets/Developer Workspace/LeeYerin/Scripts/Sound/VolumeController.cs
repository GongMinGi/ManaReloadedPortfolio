using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 개발자: 이예린
/// 
/// 슬라이더 값을 통해 BGM 또는 SFX 볼륨을 조절하는 컨트롤러
public class VolumeController : MonoBehaviour
{
    public Slider volumeSlider; // 볼륨 조절에 사용하는 슬라이더
    [Tooltip("BGM 컨트롤러 여부를 지정하는 변수 (true면 BGM, false면 SFX)")]
    [SerializeField] bool isBGMController;

    #region Unity Event
    private void Start()
    {
        // 슬라이더 값이 변경될 때마다 OnVolumeChange 함수 호출
        //volumeSlider.onValueChanged.AddListener(OnVolumeChange);

        // 슬라이더의 초기값을 AudioSource의 볼륨으로 설정
        //if (isBGMController)
        //    volumeSlider.value = GameModeManager.SoundManager.BGMVolume;
        //else
        //    volumeSlider.value = GameModeManager.SoundManager.SFXVolume;
    }
    #endregion

    /// <summary>
    /// 슬라이더 값이 변경될 때 호출되며,
    /// BGM 또는 SFX 볼륨을 변경하는 메서드
    /// </summary>
    /// <param name="value">변경된 슬라이더 값</param>
    void OnVolumeChange(float value)
    {
        // 슬라이더 값에 따라 AudioSource의 볼륨을 변경
        if (isBGMController)
            GameModeManager.SoundManager.BGMVolume = value;
        else
            GameModeManager.SoundManager.SFXVolume = value;
    }
}
