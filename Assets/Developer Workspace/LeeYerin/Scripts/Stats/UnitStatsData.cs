using UnityEngine;

namespace Game.Combat.Stats
{
    /// <summary>
    /// 개발자: 이예린
    /// 
    /// 유닛의 기본 스탯 데이터를 저장하는 ScriptableObject
    /// 현재는 기본 이동 속도(BaseMoveSpeed)와 기본 방어력(BaseDefense)를 포함
    /// 추후 추가 스탯이 확장될 가능성을 염두에 두고 설계함
    /// </summary>
    [CreateAssetMenu(fileName = "UnitStatsData", menuName = "Scriptable Objects/UnitStatsData")]
    public class UnitStatsData : ScriptableObject
    {
        [SerializeField] private float baseMoveSpeed;
        [SerializeField] private float baseDefense;

        /// <summary>
        /// 기본 이동 속도
        /// </summary>
        public float BaseMoveSpeed => baseMoveSpeed;
        /// <summary>
        /// 기본 방어력
        /// </summary>
        public float BaseDefense => baseDefense;
    }
}