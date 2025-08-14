using Game.Combat.Stats;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// * 작성자
///  - HP UI 바인딩 베이스: 이벤트 구독/슬라이더 갱신/후크 제공
///  - 플레이어/적 hp slider가 공통으로 상속받는 상위클래스
/// </summary>
public class HealthBarBinder : MonoBehaviour
{

    [SerializeField] protected UnitStats targetStat;    // 동기화 대상(플레이어/적)의 스탯
    [SerializeField] protected Slider hpSlider;         // 연결된 UI 슬라이더(0~1 비율로 사용)


    // 활성화 시: 대상의 HP 변경 이벤트에 콜백 연결
    protected virtual void OnEnable()                   // 활성화시에 액션에 함수 연결
    {
        if (targetStat != null)
            targetStat.OnHpChanged += HandleHpChanged;  // 활성화 시: 대상의 HP 변경 이벤트에 콜백 연결
    }


    // 비활성화 시: 이벤트 구독 해제(메모리/중복 호출 방지)
    protected virtual void OnDisable()                  // 비활성화시에 연결 해제
    {
        if (targetStat != null)
            targetStat.OnHpChanged -= HandleHpChanged;
    }

    // HP 변경 공통 처리: (1) 비율 계산해 슬라이더 갱신 (2) 파생 후크 호출
    protected void HandleHpChanged(float curHp, float maxHp)
    {
        hpSlider.value = maxHp <= 0f ? 0f : curHp / maxHp;      // 0분모 방지 + 0~1 정규화
        OnRatioChanged(hpSlider.value);                         // 보이기/숨기기는 파생 클래스에서
    }

    // 파생 클래스가 오버라이드해 표시 규칙(숨김/효과 등) 구현
    protected virtual void OnRatioChanged(float ratio) { }  
}
