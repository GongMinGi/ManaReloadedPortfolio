using UnityEngine;

/// <summary>
/// 개발자: 이예린
/// 
/// UI 버튼 클릭 시 지정된 SFX를 재생하는 컴포넌트
/// 
/// 버튼마다 서로 다른 효과음을 지정할 수 있으며,
/// Unity의 Button 컴포넌트 OnClick 이벤트에 연결하여 사용
/// </summary>
public class UIButtonSound : MonoBehaviour
{
    [SerializeField] private int sfxID;

    /// <summary>
    /// 지정된 SFX ID에 해당하는 효과음을 재생
    /// </summary>
    //public void PlaySFX() => GameModeManager.SoundManager.PlaySFX(sfxID);
}