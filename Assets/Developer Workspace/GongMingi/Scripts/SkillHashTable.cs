using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;



public enum E_CastingType
{
    Fire,
    Frost,
    Lightning,
    Earth,
    Lumina,
    Darkness,
}




[CreateAssetMenu(menuName = "★스킬 추가 버튼★")]
public class SkillHashTable : ScriptableObject
{

    public int currentHashID;                       // Editor에서 계산 - 저장해놓는 해시 값 (중복되지 않음)

    [Header("기획용 스킬 제조기")]
    public List<E_CastingType> CastList;            // 주문 조합
    public BaseSkill castSkill;                     // 이 조합으로 발동될 실제 스킬


    [ContextMenu("[코드 업데이트]")]
    private void UpdateSetHash()
    {
        if( CastList.Count <0)
        {
            currentHashID = 0;
        }


        currentHashID = SkillCastingManager.GetCastTypeHashTable(CastList);


        // CastSkill 누락 시 경고 표시
        if(!castSkill)
        {
            Debug.LogError("스킬 없음. 맞는지 확인하기");
        }

    }
}
