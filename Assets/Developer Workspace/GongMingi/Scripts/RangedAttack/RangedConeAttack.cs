using UnityEditor.ShaderGraph;
using UnityEngine;


/// <summary>
/// * 작성자 : 공민기
///   - 캐스팅한 원소가 불과 냉기 밖에 없는 경우 실행되는 원거리 공격
///   - 원거리공격을 시전하는 동시에, 원뿔형태의 마법 공격이 시전된다.
/// </summary>
public class RangedConeAttack : MonoBehaviour, IRangedAttack
{

    [Header("Cone Parameters")]
    [SerializeField] private float radius = 6f;         // 탐지 반경
    [SerializeField] private float angle = 60f;         // 전체 부채꼴 각도(디그리) 
    [SerializeField] private int damage = 12;           // 1회 피해량
    [SerializeField] private LayerMask enemyLayer;     // Enemy 전용 레이어 , 
    [SerializeField] private bool flatCone = true;      // Y축 높이 무시 여부


    float cosThreshold;                                 // cos(angle/2) 캐시


    void Awake() => cosThreshold = Mathf.Cos(angle * 0.5f * Mathf.Deg2Rad);


    public void ExecuteAttack(E_CastingType type)
    {
        Debug.Log("원뿔 공격이 시전되었습니다.");

        cosThreshold = Mathf.Cos(angle * 0.5f * Mathf.Deg2Rad);

        Collider[] hits = Physics.OverlapSphere(transform.position, radius, enemyLayer);

        Debug.Log($"적 개체 {hits.Length} 개 감지");
        
        Vector3 forward = transform.forward;            // 플레이어 정면 벡터 준비
        if (flatCone) forward.y = 0;
        forward.Normalize();

        // step 2) 부채꼴 판정 & step 3) 데미지 적용
        foreach ( var hit in hits )
        {
            Vector3 dir = hit.transform.position - transform.position;
            if (flatCone) dir.y = 0;
            dir.Normalize();

            if (Vector3.Dot(forward, dir) >= cosThreshold)
            {
                Debug.Log("원거리 원뿔 공격 데미지 적용");
               
                // 데미지 적용
                //if (hit.TryGetComponent(out IDamageable target))
                //    target.TakeDamage(damage);
            }      

        }

    }


    // 필요시 채널링 /dot 취소용
    public void Stop()
    {

    }


#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!enabled) return;

        Gizmos.color = new Color(1, 0.5f, 0, 0.25f);
        UnityEditor.Handles.color = Gizmos.color;
        UnityEditor.Handles.DrawSolidArc(
            transform.position,
            flatCone ? Vector3.up : transform.up,
            Quaternion.Euler(0, -angle * 0.5f, 0) * transform.forward,
            angle, radius
            );
    }
#endif

}
