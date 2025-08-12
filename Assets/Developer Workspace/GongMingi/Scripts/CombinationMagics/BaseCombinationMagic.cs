using UnityEngine;

public class BaseCombinationMagic : MonoBehaviour
{
    public virtual void ExecuteSkill()
    {
        Debug.Log("스킬 실행");
    }
}


/*
 * 모든 스킬 공통으로 사용할 쿨타임 로직 코루틴으로 구현하기
 * 
 */
