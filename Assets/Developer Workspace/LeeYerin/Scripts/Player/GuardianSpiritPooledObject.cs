using System.Collections;
using UnityEngine;

/// <summary>
/// 개발자: 이예린
/// 
/// Guardian Spirit 오브젝트 풀링용 클래스
/// 
/// 플레이어 주변에 5개의 수호령을 별 모양으로 배치하여 지속적으로 데미지를 가하는 스킬
/// </summary>
public class GuardianSpiritPooledObject : PooledObject
{
    #region Guardian Skill Configuration
    [Header("Guardian Deployment")]
    [SerializeField] float range = 2f;  // 수호령 배치 및 공격 범위
    [SerializeField] Transform[] guardianObjs = new Transform[5];   // 수호령 오브젝트 배열 (5개)

    [Header("Damage Settings")]
    [SerializeField] float damage;  // 수호령 데미지량
    [SerializeField] float tickInterval;    // 데미지 틱 간격 (초)

    [Header("Duration")]
    [SerializeField] float lifeTime;    // 스킬 지속 시간
    private float elapsedLifetime;      // 경과된 시간 추적
    private float intervalTimer;        // 데미지 틱 타이머

    private bool isEffectActive;        // 스킬 활성화 상태
    private bool isPositioned;          // 수호령 배치 완료 여부
    #endregion

    #region unity Event
    private void Update()
    {
        // 오브젝트 위치를 항상 플레이어 위치로 동기화
        transform.position = GameModeManager.Player.transform.position;

        // 스킬이 비활성화 상태면 업데이트 중단
        if (isEffectActive == false)
        {
            return;
        }

        // 경과 시간 누적
        elapsedLifetime += Time.deltaTime;

        // 지속 시간 종료 시 스킬 비활성화
        if (elapsedLifetime >= lifeTime)
        {
            isEffectActive = false;
            elapsedLifetime = 0f;

            Release();  // 스킬 종료 처리
        }
    }
    #endregion

    #region Guardian spirits positioning
    /// <summary>
    /// 수호령들을 플레이어 중심으로 별(오각형) 모양 배치
    /// </summary>
    /// <returns>배치 성공 여부 (항상 true 반환)</returns>
    private bool PositionGuardiansInStar()
    {
        // 5개의 수호령을 75도 간격으로 배치
        for (int i = 0; i < 5; i++)
        {
            // 각도 계산: 72도씩 증가. -90도부터 시작하여 위쪽이 첫 번째
            // Deg2Rad을 사용해 라디안으로 변환 (삼각함수는 라디안을 사용함)
            float angle = (i * 72f - 90f) * Mathf.Deg2Rad;

            // 단위원 좌표 계산
            float x = Mathf.Cos(angle);
            float z = Mathf.Sin(angle);

            // 단위원(-1~1)을 실제 범위로 스케일링
            float scaledX = x * range * 0.5f;
            float scaledZ = z * range * 0.5f;

            guardianObjs[i].position = transform.position + new Vector3 (scaledX, 1f, scaledZ);
        }

        return true;    // 배치 완료 표시
    }
    #endregion

    #region Hook
    protected override IEnumerator OnActivated()
    {
        // 타이머 초기화
        elapsedLifetime = 0;
        intervalTimer = tickInterval;

        // 이미 배치된 상태라면 즉시 활성화
        if (isPositioned == true)
        {
            isEffectActive = true;
            yield break;
        }

        // 가디언들을 별 모양으로 배치
        isPositioned = PositionGuardiansInStar();

        yield return new WaitUntil(() => isPositioned == true);

        // 스킬 효과 활성화
        isEffectActive = true;
    }
    #endregion
}