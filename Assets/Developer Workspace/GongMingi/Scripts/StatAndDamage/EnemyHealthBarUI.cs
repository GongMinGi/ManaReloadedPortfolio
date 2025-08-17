using Game.Combat.Stats;
using UnityEngine;


/// <summary>
/// * 작성자 : 공민기
///  - 적 HP바 전용 UI 컨트롤러: 베이스 바인더(이벤트 구독/슬라이더 갱신)를 상속
///  - hp가 최대일때는 hp를 표시하지 않고 데미지를 입은 상태에서만 현재 hp 표시
/// </summary>
public class EnemyHealthBarUI : HealthBarBinder
{

    [Header("World UI")]
    [SerializeField] private Transform followTarget;                            // hp 바가 쫒아다닐 위치
    [SerializeField] private Camera cam;                                        // 카메라 방향을 바라보로돌고 하기위한 카메라 정보
    [SerializeField] private bool hideWhenFull = true;                          // hp가 가득차 있는 경우 표시하지 않음
    [SerializeField] private Canvas worldCanvas;                                // 몬스터 hp바를 표시할 canvas
    [SerializeField] private CanvasGroup group;
    [SerializeField] private float hideThreshold = 0.999f;
    [SerializeField] private float showthreshold = 0.995f; 

    public void BindTarget(UnitStats targetStatInform, Transform followingPos)
    {
        targetStat = targetStatInform;                  // unitstat 정보를 받는 매개변수
        followTarget = followingPos;                    // 적 머리 위 hpbar의 트랜스폼을 받아옴

        if (targetStat != null)
            targetStat.OnHpChanged += HandleHpChanged;  // 활성화 시: 대상의 HP 변경 이벤트에 콜백 연결

        Debug.Log("적체력 바 활성화 및 이벤트 등록");

    }

    // 활성화 시: (1) 베이스 구독 (2) 스폰 직후 상태 강제 동기화 (3) 카메라 참조 확보
    protected override void OnEnable()
    {


        if (targetStat != null)
            HandleHpChanged(targetStat.HP, targetStat.StatsData.BaseHP);        // 소환시 풀피이므로 hpchanged를 호출해서 채력바를 숨겨준다.

        if (!cam) cam = Camera.main;

    }


    // 좌표/회전은 LateUpdate에서 최신 트랜스폼 결과 반영 후 처리
    private void LateUpdate()
    {
        if (!cam) return;       // 카메라 미할당 시 return

        Transform followingPos = followTarget ? followTarget : (targetStat ? targetStat.transform : transform);  // followTarget이 있으면 처넣고, 없으면 unitstat에서 가져다 넣고, 아니면 그냥 transform
        if (followingPos) transform.position = followingPos.position;         // 기준 위치가 유효하면, worldOffset 만큼 머리 위로 올려서 배치


        transform.forward = cam.transform.forward;      // 항상 카메라를 정면으로 바라보게 함(간단/저비용)
    }


    // HP 비율 변화 시 호출: 보이기/숨기기 규칙 적용
    protected override void OnRatioChanged(float ratio)     // 체력 비율이 바뀔때마다 호출
    {
        if (!group) return;

        if (hideWhenFull && ratio >= hideThreshold)                           // 풀피 숨김 옵션이 true일때만
        {
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;
            //worldCanvas.enabled = ratio < 0.999f;   // 풀피일때는 숨김

        }
        else
        {
            group.alpha = 1f;
            //worldCanvas.enabled = false;

        }
    }



}
