using UnityEngine;

namespace Game.Combat.EnemyAttack.SO
{
    /// <summary>
    /// 개발자: 이예린
    /// 
    /// 직사각형(Rectangle) 형태 공격 범위 데이터를 정의하는 ScriptableObject
    /// AttackRangeData를 상속하며, 길이(length)와 너비(width)를 기반으로 직사각형 범위 공격 인스턴스를 생성
    /// 
    /// 생성된 RectAttackRange 인스턴스는 BaseAttack에서 공격 로직 수행 시 사용됨
    /// </summary>
    [CreateAssetMenu(fileName = "RectAttackRangeData", menuName = "Scriptable Objects/Enemy/Attack/Range/Rect")]
    public class RectAttackRangeData : AttackRangeData
    {
        [Header("직사각형 공격 범위 설정")]
        [Tooltip("직사각형 범위의 길이")]
        [SerializeField] private float length;

        [Tooltip("직사각형 범위의 너비")]
        [SerializeField] private float width;

        /// <summary>
        /// 직사각형 공격 범위 인스턴스를 생성
        /// </summary>
        /// <returns>RectAttackRange 타입의 IAttackRange 인스턴스</returns>
        public override IAttackRange CreateInstance()
        {
            return new RectAttackRange(length, width);
        }
    }
}
