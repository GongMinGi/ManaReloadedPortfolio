using UnityEngine;

namespace Game.Combat.EnemyAttack
{
    /// <summary>
    /// 개발자: 이예린
    /// 
    /// 적의 공격 범위를 정의하는 인터페이스
    /// 공격자가 특정 타겟을 공격할 수 있는 범위 내에 있는지 판정하는 메서드를 제공
    /// </summary>
    public interface IAttackRange
    {
        /// <summary>
        /// 공격자가 타겟을 공격할 수 있는 범위 내에 있는지 판정
        /// </summary>
        /// <param name="attacker">공격자 Transform</param>
        /// <param name="target">타겟 Transform</param>
        /// <returns>범위 내에 있으면 true, 아니면 false</returns>
        bool IsPlayerInRange(Transform attacker, Transform target);
    }

    /// <summary>
    /// 개발자: 이예린
    /// 
    /// 직선(Line) 형태 공격 범위
    /// 공격자의 전방 방향으로 distance 내에 있는 경우만 공격 가능
    /// </summary>
    public class LineAttackRange : IAttackRange
    {
        private float distance;

        public LineAttackRange(float distance) => this.distance = distance;

        public bool IsPlayerInRange(Transform attacker, Transform target)
        {
            Vector3 dir = target.position - attacker.position;
            return dir.magnitude <= distance && Vector3.Dot(attacker.forward, dir.normalized) > 0.999f;
        }
    }

    /// <summary>
    /// 개발자: 이예린
    /// 
    /// 직사각형(Rect) 형태 공격 범위
    /// 공격자 기준 전방 방향 직사각형 영역 내에 타겟이 있으면 공격 가능
    /// </summary>
    public class RectAttackRange : IAttackRange
    {
        private float length;
        private float width;

        public RectAttackRange(float length, float width)
        {
            this.length = length;
            this.width = width;
        }

        public bool IsPlayerInRange(Transform attacker, Transform target)
        {
            Vector3 dir = target.position - attacker.position;
            Vector3 localPos = attacker.InverseTransformPoint(target.position);
            return Mathf.Abs(localPos.x) <= width * 0.5f && localPos.z >= 0 && localPos.z <= length;
        }
    }

    /// <summary>
    /// 개발자: 이예린
    /// 
    /// 원형(Circle) 형태 공격 범위
    /// 공격자 주변 반경 radius 안에 타겟이 있으면 공격 가능
    /// </summary>
    public class CircleAttackRange : IAttackRange
    {
        private float radius;

        public CircleAttackRange(float radius) => this.radius = radius;

        public bool IsPlayerInRange(Transform attacker, Transform target)
        {
            return Vector3.Distance(attacker.position, target.position) <= radius;
        }
    }
}
