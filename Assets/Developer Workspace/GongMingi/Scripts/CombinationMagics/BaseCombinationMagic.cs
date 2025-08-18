using UnityEngine;

public class BaseCombinationMagic : MonoBehaviour
{

    public struct Context
    {
        public Transform caster;        // 시전자(플레이어) 트랜스폼
        public MonoBehaviour coroutineRunner;   // 코루틴 실행 주체 ( 대개 playercontroller 자신)
    }

    public virtual void ExecuteSkill()
    {
        Debug.Log("스킬 실행");
    }
}


/*
 * 모든 스킬 공통으로 사용할 쿨타임 로직 코루틴으로 구현하기
 * 
 */
