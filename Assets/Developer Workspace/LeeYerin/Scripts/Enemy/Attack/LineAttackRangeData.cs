using UnityEngine;

namespace Game.Combat.EnemyAttack.SO
{
    /// <summary>
    /// 개발자: 이예린
    /// 
    /// 직선 형태(Line) 공격 범위 데이터를 정의하는 ScriptableObject
    /// AttackRangeData를 상속하며, 거리(distance)를 기반으로 직선 범위 공격 인스턴스를 생성
    /// 
    /// 생성된 LineAttackRange 인스턴스는 BaseAttack에서 공격 로직 수행 시 사용됨
    /// </summary>
    [CreateAssetMenu(fileName = "LineAttackRangeData", menuName = "Scriptable Objects/Enemy/Attack/Range/Line")]
    public class LineAttackRangeData : AttackRangeData
    {
        [Header("직선 공격 범위 설정")]
        [Tooltip("공격 가능한 최대 거리")]
        [SerializeField] private float distance;

        /// <summary>
        /// 직선 공격 범위 인스턴스를 생성
        /// </summary>
        /// <returns>LineAttackRange 타입의 IAttackRange 인스턴스</returns>
        public override IAttackRange CreateInstance()
        {
            return new LineAttackRange(distance);
        }
    }
}
