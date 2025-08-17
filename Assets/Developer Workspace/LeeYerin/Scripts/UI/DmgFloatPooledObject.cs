using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 개발자: 이예린
/// 
/// DamageText UI 동작이 구현된 PooledObject 클래스
/// 
/// - 카메라를 바라보도록 회전
/// - Target 위치를 따라 이동
/// - 표시 시간과 페이드 아웃 처리
/// - 활성화/비활성화 시 초기화
/// - 피해량 갱신과 DOTween 시퀀스 처리
/// </summary>
public class DmgFloatPooledObject : PooledObject
{
    #region UI Components
    [Tooltip("TextMeshPro component displaying the damage value")]
    [SerializeField] private TMP_Text mDamageLabel;     // 피해량을 표시하는 텍스트 컴포넌트
    #endregion

    #region Position Settings
    [Header("Position Settings")]
    /// <summary>
    /// 데미지 텍스트의 세로 위치 오프셋
    /// </summary>
    [Tooltip("Vertical offset for positioning the damage text above the target.")]
    [SerializeField] private float offsetY = 2.5f;
    #endregion

    #region Timers
    [Header("Timers")]
    [Tooltip("Total time the damage label remains visible")]
    [SerializeField] private float damageVisibleTime = 1.5f;    // 피해 텍스트가 화면에 표시되는 시간

    [Tooltip("Time it takes for the damage label to fade out")]
    [SerializeField] private float fadeTime = 0.5f;     // 피해 텍스트가 사라지는 페이드 아웃 시간

    [Tooltip("Remaining time before the damage label disappears")]
    [SerializeField] private float timeLeft;    // 피해 텍스트가 사라지기 전 남은 시간
    #endregion

    #region Damage Data
    [Header("Damage Data")]
    [Tooltip("Damage value to display")]
    [SerializeField] private float damage;      // 표시할 피해량 값

    [Tooltip("The target transform associated with this damage label")]
    public Transform Target { get; set; }        // 피해 텍스트가 따라갈 대상 Transform
    #endregion

    #region Animation
    [Header("Animation")]
    [Tooltip("DOTween sequence controlling the label's animation")]
    private Sequence seq = null;         // 피해 텍스트 애니메이션을 제어하는 DOTween 시퀀스
    #endregion

    private Transform cam;  // 바라볼 카메라

    #region Unity Event
    private void Awake()
    {
        if (!IsUI)
            IsUI = true; // UI로 설정 안 되어 있으면 UI로 지정

        cam = Camera.main.transform;    // 바라볼 카메라 세팅

        transform.localScale = Vector3.one * 0.1f; // 초기 크기 설정
    }

    private void Update()
    {
        if (timeLeft <= 0f) return;

        timeLeft -= Time.deltaTime; // 남은 시간 감소
        if (timeLeft <= 0f)
        {
            seq?.Kill(); // 이전 시퀀스 종료
            seq = DOTween.Sequence(); // 새 시퀀스 생성
            GameModeManager.UIManager.FadeOut
                (mDamageLabel,
                fadeTime,
                seq,
                () => Release()); // 페이드 아웃 후 해제 콜백

            seq.Play(); // 시퀀스 시작
        }
    }

    private void LateUpdate()
    {
        if (Target == null) return; // 대상 없으면 종료
        transform.position = Target.position + Vector3.up * offsetY; // 대상 위에 위치

        transform.LookAt(cam); // 카메라 바라보도록 회전
        transform.Rotate(0, 180f, 0); // 텍스트 정방향 조정
    }
    #endregion

    // 활성화 시 초기화
    protected override IEnumerator OnActivated()
    {
        timeLeft = damageVisibleTime; // 표시 시간 초기화
        damage = 0f; // 누적 피해량 초기화

        if (mDamageLabel.alpha != 1f)
            mDamageLabel.alpha = 1f; // 텍스트 투명도 초기화
        yield break;
    }

    // 비활성화 시 처리
    protected override void OnDeactivated()
    {
        damage = 0f; // 피해량 초기화
        Target = null; // 대상 제거

        mDamageLabel.DOKill(); // 안전하게 트윈 종료
        seq = null; // 시퀀스 null 처리
    }

    /// <summary>
    /// 피해량 텍스트 UI를 갱신하는 메서드
    /// 
    /// - 현재 피해량에 전달받은 피해를 누적
    /// - 페이드 타이머를 초기화
    /// - 진행 중인 애니메이션 시퀀스를 종료
    /// </summary>
    /// <param name="damage">입은 데미지</param>
    public void SetDamageText(float damage)
    {
        mDamageLabel.text = $"{this.damage += damage}"; // 누적 피해량 표시

        if (seq != null)
        {
            seq.Kill(); // 기존 시퀀스 종료
            seq = null; // 시퀀스 null 처리
            mDamageLabel.alpha = 1f; // 텍스트 투명도 초기화
        }

        timeLeft = damageVisibleTime; // 남은 시간 갱신
    }

}