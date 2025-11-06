using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public enum E_CastingType
{
    Earth,
    Thunder,
    Light,
    Darkness, 
    Fire,
    Cold,
    Water,
    Air,

    None,
    // 미구현
}

/// <summary>
/// 특정 원소 조합과 그에 대응하는 스킬(BaseSkill)을 하나의 에셋으로 보관하는 ScriptableObject입니다.
/// - CastList에 조합을, castSkill에 실제 스킬을 지정하고 Editor에서 UpdateSetHash()로 해시값을 미리 계산-저장합니다.
/// - 런타임에서는 SkillCastingManager가 이 해시를 키로 사용해 빠르게 스킬을 조회합니다.
/// </summary>
[CreateAssetMenu(menuName = "★스킬 추가 버튼★")]
public class SkillHashTable : ScriptableObject
{
    public int currentHashID;                       // Editor에서 계산 - 저장해놓는 해시(고유) 값 (중복되지 않음)

    [Header("기획용 스킬 제조기")]
    public List<E_CastingType> CastList;            // 주문 조합
    public BaseCombinationMagic castSkill;          // 이 조합으로 발동될 실제 스킬

    /// <summary>
    /// **Editor 전용** 해시값 갱신 유틸리티.
    /// CastList(원소 조합)를 <see cref="SkillCastingManager.GetCastTypeHashTable(List{E_CastingType})"/>
    /// 로 변환해 currentHashID에 저장한다.  
    /// - CastList가 비어 있으면 0을 기록해 오류 상황을 식별.  
    /// - castSkill이 할당되지 않았을 경우 개발자에게 경고 로그를 출력.  
    /// 메뉴 항목(우클릭 Context Menu)으로 노출돼 수동으로 호출할 수 있다.
    /// </summary>
    [ContextMenu("[코드 업데이트]")]
    private void UpdateSetHash()
    {
        if( CastList.Count <0)              // 현재 스크립터블 오브젝트의 캐스팅 된 원소가 없으면
        {
            currentHashID = 0;              // 조합마법이 만들어질 수 없으므로 고유번호를 0으로 할당
        }

        currentHashID = SkillCastingManager.GetCastTypeHashTable(CastList);     // 등록한 원소 조합 리스트를 매니저에게 넘겨서 고유번호를 받아온다.

        // CastSkill 누락 시 경고 표시
        if(!castSkill)                                          // 해당 조합으로 발동시킬 스킬이 할당되어 있지 않은 경우 오류를 출력한다. 
        {
            Debug.LogError("스킬 없음. 맞는지 확인하기");      
        }
    }
}
