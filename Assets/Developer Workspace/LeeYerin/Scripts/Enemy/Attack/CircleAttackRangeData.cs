using UnityEngine;

namespace Game.Combat.EnemyAttack.SO
{
    /// <summary>
    /// 개발자: 이예린
    /// 
    /// 원형(Circle) 형태 공격 범위 데이터를 정의하는 ScriptableObject
    /// AttackRangeData를 상속하며, 반지름(radius)을 기반으로 원형 범위 공격 인스턴스를 생성
    /// 
    /// 생성된 CircleAttackRange 인스턴스는 BaseAttack에서 공격 로직 수행 시 사용됨
    /// </summary>
    [CreateAssetMenu(fileName = "CircleAttackRangeData", menuName = "Scriptable Objects/Enemy/Attack/Range/Circle")]
    public class CircleAttackRangeData : AttackRangeData
    {
        [Header("원형 공격 범위 설정")]
        [Tooltip("공격 가능한 최대 반지름")]
        [SerializeField] private float radius;

        /// <summary>
        /// 원형 공격 범위 인스턴스를 생성
        /// </summary>
        /// <returns>CircleAttackRange 타입의 IAttackRange 인스턴스</returns>
        public override IAttackRange CreateInstance()
        {
            return new CircleAttackRange(radius);
        }
    }
}
