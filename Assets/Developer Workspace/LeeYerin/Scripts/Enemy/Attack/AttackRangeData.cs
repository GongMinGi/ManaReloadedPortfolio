using UnityEngine;

namespace Game.Combat.EnemyAttack.SO
{
    /// <summary>
    /// 개발자: 이예린
    /// 
    /// 공격 범위 데이터를 정의하는 추상 클래스
    /// ScriptableObject를 상속받아 공격 범위 설정용 데이터 자산으로 사용
    /// 
    /// 구체적인 범위 로직은 IAttackRange 구현체에서 처리하며,
    /// CreateInstance()를 통해 범위 인스턴스를 생성할 수 있음
    /// </summary>
    public abstract class AttackRangeData : ScriptableObject
    {
        /// <summary>
        /// IAttackRange 타입의 범위 인스턴스를 생성
        /// 구체적인 범위 로직은 상속 클래스에서 구현
        /// </summary>
        public abstract IAttackRange CreateInstance();
    }
}