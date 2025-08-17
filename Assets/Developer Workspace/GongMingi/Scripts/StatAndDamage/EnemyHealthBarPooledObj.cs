using Game.Combat.Stats;
using UnityEngine;


/// <summary>
/// *작성자: 공민기
///  - 적 체력바를 오브젝트 풀링하기 위애 pooledobject를 상속받은 스크립틀르 붙여줌
///  - worldspace canvas 크기에 맞춰 크기를 0.1로 설정
///  - unitstats, 체력바를 매개로 넘겨서 오브젝트 생성
/// </summary>
public class EnemyHealthBarPooledObj : PooledObject
{
    [SerializeField] EnemyHealthBarUI targetHealthBar;
    [SerializeField] UnitStats targetStatus;
    [SerializeField] Transform enemyHpPos;

    public void Setup(UnitStats stat, Transform follow)
    {
        targetStatus = stat;                            // 데미지를 연동할 적 스텟 정보
        enemyHpPos = follow;                            // 적 체력바를 표시할 위치 정보 
        targetHealthBar.BindTarget(targetStatus, enemyHpPos);   // 정보전달 메서드
        this.transform.localScale = Vector3.one * 0.1f; // 캔버스 크기에 맞춰서 체력바 비율 조절
    }


}
