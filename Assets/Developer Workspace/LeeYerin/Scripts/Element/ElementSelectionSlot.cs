using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 개발자: 이예린
/// 
/// 플레이어가 원소를 선택할 수 있는 UI 슬롯을 담당하는 컴포넌트
/// 
/// 슬롯은 특정 원소 타입(E_CastingType)을 표시하며, 클릭 시 해당 원소를 플레이어 키에 바인딩 시도
/// </summary>
public class ElementSelectionSlot : MonoBehaviour
{
    [SerializeField] E_CastingType castingType;
    [SerializeField] Image slot;

    private void Start()
    {
        slot.sprite = GameModeManager.UIManager.GetElementSprite(castingType);
    }

    /// <summary>
    /// 슬롯이 클릭되었을 때 원소 바인딩을 시도하는 메서드
    /// </summary>
    public void TrySetElementBinding()
    {
        if (GameModeManager.ElementManager.SetElementBinding(castingType) == false)
        {
            Debug.LogWarning("올바르지 않은 키 세팅을 가진 원소 슬롯에 대한 세팅 시도입니다. 원소 슬롯에 할당된 키 세팅을 확인해주세요.");
        }

        // QA Room일 경우
        if (GameModeManager.QARoomManager != null)
        {
            GameModeManager.ElementManager.ApplyElementBindingsToPlayer(GameModeManager.Player);
            GameModeManager.ElementManager.TargetElementSlot = null;

            // 스킬 Icon 필터가 활성화 상태일 경우
            if (GameModeManager.QARoomManager.IsFilterOn == true)
            {
                GameModeManager.SkillCastingManager.ApplySkillFilter();
            }
        }
    }
}