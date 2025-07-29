using UnityEngine;


/// <summary>
/// * 작성자 : 공민기
///  - 원뿔 범위에 휘두르기 즉발 공격
///  - 원거리 원뿔 즉발 공격과 완전히 동일한 로직
///  - 심각한 코드중복.. 추후 인터페이스의 의미를 퇴색시키지 않는 방법으로 원거리공격 로직과 통합 필요
/// </summary>
public class MeleeConeAttack : MonoBehaviour
{
    #region Field And Property

    [Header("Cone Parameters")]
    [SerializeField] private float radius = 6f;         // 탐지 반경
    [SerializeField] private float angle = 60f;         // 전체 부채꼴 각도(디그리) 
    [SerializeField] private int damage = 12;           // 1회 피해량
    [SerializeField] private LayerMask enemyLayer;      // Enemy 전용 레이어 , 
    [SerializeField] private bool flatCone = true;      // Y축 높이 무시 여부


    float cosThreshold;                                 // cos(angle/2) 캐시

    #endregion endregion

    #region Unity Event
    void Awake() => cosThreshold = Mathf.Cos(angle * 0.5f * Mathf.Deg2Rad); // 부채꼴의 각의 절반.

    #endregion

    #region IRangedAttack Implementation

    /// <summary>
    /// 구체범위에 들어온 적을 레이어를 통해 감지
    /// 적의 방향벡터와 플레이어의 정면벡터를 내적시켜, 원뿔모양의 범위에 들어오는지 판단
    /// 들어가는 적에게만 공격 적용
    /// </summary>
    /// <param name="type"></param>
    public void ExecuteAttack(E_CastingType type)
    {
        Debug.Log("원뿔 공격이 시전되었습니다.");

        cosThreshold = Mathf.Cos(angle * 0.5f * Mathf.Deg2Rad);             // 부채꼴 공격범위의 절반을 감지범위로 결정

        Collider[] hits = Physics.OverlapSphere(
            transform.position, 
            radius, 
            enemyLayer, 
            QueryTriggerInteraction.Ignore);     // 구체 형태의 범위에 들어가 있는 적 개체를 감지

        Debug.Log($"적 개체 {hits.Length} 개 감지");

        Vector3 forward = transform.forward;                                // 플레이어 정면 벡터 준비
        if (flatCone) forward.y = 0;                                        // 벡터 y축 높이 무시
        forward.Normalize();                                                // 플레이어 정면 벡터를 정규화

        // step 2) 부채꼴 판정 & step 3) 데미지 적용
        foreach (var hit in hits)
        {
            Vector3 dir = hit.transform.position - transform.position;      // 플레이어 - 적 방향 벡터 구하기
            if (flatCone) dir.y = 0;                                        // y축 높이 무시
            dir.Normalize();                                                // 방향벡터 정규화

            if (Vector3.Dot(forward, dir) >= cosThreshold)                  // 적과 플레이어의 내적값(코사인값) 이  threshold보다 큰 경우에만 적용
            {
                Debug.Log("원거리 원뿔 공격 데미지 적용");

                // 데미지 적용
                if (hit.TryGetComponent(out IDamageable target))
                    target.TakeDamage(damage);
            }

        }

    }

    // 즉발형 스킬이므로 별도 취소 로직 필요 없음
    public void Stop() { }

    #endregion

}
