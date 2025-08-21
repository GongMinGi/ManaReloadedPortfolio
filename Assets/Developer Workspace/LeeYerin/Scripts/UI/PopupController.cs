using UnityEngine;

/// <summary>
/// 개발자: 이예린
/// 
/// 개별 팝업 UI의 동작을 제어하는 컨트롤러 클래스
/// 
/// UIManager를 통해 팝업의 열기/닫기를 요청하며,
/// 팝업 본체(popupUI)와 그 상위에 있는 배경(Backdrop)을 관리함
/// </summary>
public class PopupController : MonoBehaviour
{
    [SerializeField] GameObject popupUI;    // 실제 팝업 UI GameObject (Canvas 하위)
    [SerializeField] GameObject backdrop;

    /// <summary>
    /// 팝업 UI GameObject
    /// </summary>
    public GameObject PopupUI => popupUI;

    /// <summary>
    /// 반투명 블러 처리된 Backdrop의 GameObject
    /// </summary>
    public GameObject Backdrop => backdrop;

    #region Request Open / Close
    /// <summary>
    /// UIManager에 팝업 열기를 요청하는 메서드
    /// 스택 관리 및 UI 활성화는 UIManager에서 처리됨
    /// </summary>
    public void RequestOpen()
    {
        GameModeManager.UIManager.OpenPopup(this);
    }

    /// <summary>
    /// UIManager에 팝업 닫기를 요청하는 메서드
    /// 가장 최근의 팝업이 닫히며, 스택에서 제거됨
    /// </summary>
    public void RequestClose()
    {
        GameModeManager.UIManager.ClosePopup();
    }
    #endregion
}