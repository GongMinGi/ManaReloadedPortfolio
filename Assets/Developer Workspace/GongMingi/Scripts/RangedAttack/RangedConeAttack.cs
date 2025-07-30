using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;


/// <summary>
/// * 작성자 : 공민기
///   - 불 / 냉기 속성 전용 '즉발 부채꼴' 공격
///   - 캐스팅한 원소가 불과 냉기 밖에 없는 경우 실행되는 원거리 공격
///   - 원거리공격을 시전하는 동시에, 원뿔형태의 마법 공격이 시전된다.
/// </summary>
public class RangedConeAttack : MonoBehaviour, IRangedAttack, IRequireAttackContext, IAttackSignals
{

    #region Field And Property
    [Header("VFX Setting")]     // 구현: 이예린
    [SerializeField] VFXObject fireVFX;

    [Header("Cone Parameters")]
    [SerializeField] private float radius = 6f;         // 탐지 반경
    [SerializeField] private float angle = 60f;         // 전체 부채꼴 각도(디그리) 
    //[SerializeField] private int damage = 12;           // 1회 피해량
    [SerializeField] private bool flatCone = true;      // Y축 높이 무시 여부


    [Header("Hold & Tick")]
    [SerializeField] private float maxDuration = 4f;
    [SerializeField] private float tickInterval = 0.25f;
    [SerializeField] private int damagePerTick = 60;
    [SerializeField] private LayerMask enemyLayer;      // Enemy 전용 레이어 , 

    [Header("Visualization")]
    [SerializeField] private Transform muzzle;          // 원뿔 시작 지점(없으면 this)
    [SerializeField] private LineRenderer lr;           // 원뿔 시각화용 라인렌더러
    [SerializeField] private int arcSegments = 36;       // 호(arc) 해상도


    private RangedAttackContext _ctx;
    private Coroutine coneAttackRoutine;
    private bool isFiring;
    private float cosThreshold;                                 // cos(angle/2) 캐시
    [SerializeField] private readonly Collider[] hitObjects = new Collider[64];  // NonAlloc 버퍼



    public event Action Started;
    public event Action<float> Progress;
    public event Action Ended;
    public event Action Interrupted;

    #endregion endregion

    #region Unity Event
    void Awake()
    {
        if (!lr) lr = GetComponent<LineRenderer>();
        if (!muzzle) muzzle = transform;

        lr.useWorldSpace = true;
        lr.loop = false;
        lr.enabled = false;
        

        cosThreshold = Mathf.Cos(angle * 0.5f * Mathf.Deg2Rad); // 부채꼴의 각의 절반.

    }


    private void OnDisable()
    {
        if (lr) lr.enabled = false;
    }

    #endregion



    public void BindContext(RangedAttackContext ctx)
    {
        _ctx = ctx;
        if (!muzzle && _ctx != null && _ctx.DefaultMuzzle) muzzle = _ctx.DefaultMuzzle;
    }

    #region IRangedAttack Implementation

    /// <summary>
    /// 구체범위에 들어온 적을 레이어를 통해 감지
    /// 적의 방향벡터와 플레이어의 정면벡터를 내적시켜, 원뿔모양의 범위에 들어오는지 판단
    /// 들어가는 적에게만 공격 적용
    /// </summary>
    /// <param name="type"></param>
    public void ExecuteAttack(E_CastingType type )
    {
        if (isFiring) return;
        coneAttackRoutine = StartCoroutine(FireCone());

    }

    // 즉발형 스킬이므로 별도 취소 로직 필요 없음
    public void Stop() 
    {
        if (!isFiring) return;
        StopCoroutine(coneAttackRoutine);
        isFiring = false;
        if (lr) lr.enabled = false;
        Interrupted?.Invoke();
        
    }

    #endregion


    private IEnumerator FireCone()
    {
        isFiring = true;
        lr.enabled = true;
        Started?.Invoke();
        fireVFX.Play();         // 불 VFX 실행

        float startTime = Time.time;

        //bool sendProgress = false;      // 추후 공격중 전달할 이벤트가 있으면 사용

        while(Mouse.current.leftButton.isPressed && Time.time - startTime <maxDuration )
        {
            Vector3 origin = muzzle.position;
            Vector3 forward = muzzle.forward;
            Vector3 axis = flatCone ? Vector3.up : muzzle.up;       // 라인렌더러용 매개변수

            if (flatCone)
            {
                origin.y = muzzle.position.y;
                forward.y = 0f;
                forward = forward.sqrMagnitude > 1e-6f ? forward.normalized : transform.forward; // 라인렌더러용 매개변수
            }

            UpdateConeLine(origin, forward, axis);                  // 시각화 업데이트


            int count = Physics.OverlapSphereNonAlloc(
                origin,
                radius,
                hitObjects,
                enemyLayer,
                QueryTriggerInteraction.Ignore);


            for ( int i = 0; i< count; i++)
            {
                Collider detectedEnemyCollider = hitObjects[i];
                if (!detectedEnemyCollider) continue;

                Vector3 targetDir = detectedEnemyCollider.transform.position - origin;
                if (flatCone) targetDir.y = 0f;

                //길이 0 방지
                float distanceToTarget = targetDir.sqrMagnitude;
                if (distanceToTarget < 1e-6f) continue;

                targetDir /= Mathf.Sqrt(distanceToTarget);


                if(Vector3.Dot(forward, targetDir) >= cosThreshold)
                {
                    if(detectedEnemyCollider.TryGetComponent(out IDamageable target))
                    {
                        target.TakeDamage(damagePerTick);
                    }
                }
            }

            yield return new WaitForSeconds(tickInterval);
        }

        lr.enabled = false; // 시각화 끄기
        isFiring = false;       // 상태 해제
        fireVFX.Stop();         // 불 VFX 종료
        Ended?.Invoke();
    }



    private void UpdateConeLine(Vector3 origin, Vector3 forward, Vector3 axis)
    {

        int vertexCount = arcSegments + 2;        // 원점 + 호 ( arcSegments + 1 개 점 ) 
        if(lr.positionCount != vertexCount)
            lr.positionCount = vertexCount;


        lr.SetPosition(0, origin);                  // 시작점: 공격 시작점

        float half = angle * 0.5f;
        float step = angle / arcSegments;

        // -half(왼쪽 끝) ~ +half(오른쪽 끝)까지 step 간격으로 회전하며 호 점 생성
        for (int i = 0; i <= arcSegments; ++i)
        {
            float a = -half + step * i;                          // 현재 세그먼트의 각도
            Vector3 dir = Quaternion.AngleAxis(a, axis) * forward;// 축(axis) 기준으로 forward를 회전
            Vector3 p = origin + dir.normalized * radius;         // 회전된 방향으로 반경만큼 떨어진 점
            lr.SetPosition(i + 1, p);
        }

        // 마지막 포인트는 다시 원점으로 돌아와 부채꼴을 닫는다.
        lr.SetPosition(vertexCount - 1, origin);

    }

}
