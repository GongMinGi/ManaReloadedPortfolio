using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 개발자: 이예린
/// 
/// ElementManager의 정보를 받아, 담당 키에 대응하는 원소 아이콘을 표시하는 컴포넌트
/// 
/// 표현(View) 레벨에서의 역할만 담당
/// </summary>
public class ElementSlot : MonoBehaviour
{
    [SerializeField] PreGameFlowManager gameFlowManager;
    [SerializeField] Image  elementIcon;    // 슬롯에 표시되는 원소 아이콘 이미지
    public Image Slot => elementIcon;

    [SerializeField] private E_CastingType curType;            // 현재 할당된 속성
    [SerializeField] private Key key;                          // 이 슬롯이 담당하는 캐시
    public Key Key => key;

    #region Unity Event
    private void Start()
    {
        SetElementToThisSlot();
    }
    #endregion

    public void OnClickEquipedElement()
    {
        gameFlowManager.OpenElementList(this);
        GameModeManager.ElementManager.targetElementSlot = this;
    }

    /// <summary>
    /// ElementManager에 저장된 데이터 기반으로  
    /// 현재 슬롯의 아이콘 이미지를 갱신을 요청하는 메서드
    /// </summary>
    public void SetElementToThisSlot()
    {
        GameModeManager.ElementManager.UpdateElementSlots(key, elementIcon);
    }
}
