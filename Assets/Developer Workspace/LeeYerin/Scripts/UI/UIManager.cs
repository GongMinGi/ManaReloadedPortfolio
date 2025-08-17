using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 개발자: 이예린
/// 
/// UI 전환 효과(Fade In/Out) 및 팝업 UI의 스택 기반 관리 기능 둥 UI를 위한 기능 제공하는 메니저 클래스
/// </summary>
public class UIManager : MonoBehaviour
{
    private static UIManager instance;

    [SerializeField] Image fadeUI;      // 전체 화면에 적용될 페이드 이미지
    [SerializeField] float fadeTime;    // 페이드 효과에 소요되는 시간

    private Stack<PopupController> popupHistory = new();    // 팝업 UI 스택

    public bool IsFirstLaunch => !fadeUI.gameObject.activeSelf;     // 게임 실행 후 첫 진입인지 여부
    public bool LoadIntoLoadoutUI { get; set; } = false;    // 로드아웃으로의 이동인지 여부

    Sequence sequenceFadeIn;
    Sequence sequenceFadeOut;

    [Header("Enemy DmgText Object Pool Settings")]
    [SerializeField] DmgFloatPooledObject dmgTextObj;
    [SerializeField] int size;
    [SerializeField] int capacity;
    private bool isDmgTextPoolExist;


    [Header("Enemy Health Bar Pool Setting")]
    [SerializeField] EnemyHealthBarPooledObj enemyHealthBarPooledObj;
    [SerializeField] int initialHealBarCnt;
    [SerializeField] int maxHealthBarCnt;
    private bool isEnemyHealthBarPoolExist;

    #region Unity Event
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            GameModeManager.UIManager = instance;
            DontDestroyOnLoad(gameObject);  // 씬 전환 시 유지
        }
        else
            Destroy(gameObject);
    }
    #endregion

    #region Fade in/out
    /// <summary>
    /// 화면이 점점 밝아지는 페이드 인 효과를 구현한 메서드
    /// </summary>
    /// <param name="onComplete">페이드 인 완료 후 실행할 콜백</param>
    public void FadeIn(Action onComplete = null)
    {
        if (sequenceFadeIn == null)
        {
            sequenceFadeIn = DOTween.Sequence();

            sequenceFadeIn.Append(fadeUI.DOFade(0.0f, fadeTime))
                .SetAutoKill(false)
                .OnComplete(() => 
                {
                    fadeUI.gameObject.SetActive(false);     // 페이드 완료 시 비활성화
                    onComplete?.Invoke();
                });
        }
        else
        {
            sequenceFadeIn.OnComplete(() =>
            {
                fadeUI.gameObject.SetActive(false);
                onComplete?.Invoke();
            });
            sequenceFadeIn.Restart();   // 기존 시퀀스 재사용
        }
    }

    /// <summary>
    /// 화면을 점점 어둡게 만드는 페이드 아웃 효과
    /// </summary>
    /// <param name="onComplete">페이드 아웃 완료 후 실행할 콜백</param>
    public void FadeOut(Action onComplete = null)
    {
        if (sequenceFadeOut == null)
        {
            sequenceFadeOut = DOTween.Sequence();

            sequenceFadeOut.Append(fadeUI.DOFade(1.0f, fadeTime))
                .SetAutoKill(false)
                .OnStart(() =>
                {
                    fadeUI.gameObject.SetActive(true);      // 시작 시 UI 활성화
                })
                .OnRewind(() =>                             // 시퀀스 되감기 대비 처리
                {
                    fadeUI.gameObject.SetActive(true);  
                })
                .OnComplete(() =>
                {
                    onComplete?.Invoke();
                });
        }
        else
        {
            // 시퀀스가 이미 생성되어 있을 경우, 이전에 설정된 OnComplete 콜백이 남아있을 수 있으므로
            // 외부에서 새로 전달된 onComplete를 반영하기 위해 OnComplete를 다시 설정함
            sequenceFadeOut.OnComplete(() =>
            {
                onComplete?.Invoke();
            });
            sequenceFadeOut.Restart();    // 기존 시퀀스 재사용
        }
    }

    /// <summary>
    /// 텍스트를 페이드 아웃 시키는 메서드
    /// - 지정한 시간 동안 텍스트 투명도를 0으로 변화
    /// - 페이드 후 짧은 간격을 둠
    /// - 완료 콜백이 있으면 실행
    /// </summary>
    /// <param name="text">페이드할 TMP_Text 컴포넌트</param>
    /// <param name="duration">페이드 지속 시간</param>
    /// <param name="seq">DOTween 시퀀스</param>
    /// <param name="onComplete">페이드 완료 후 실행할 콜백 (선택)</param>
    public void FadeOut(TMP_Text text, float duration, Sequence seq, Action onComplete = null)
    {
        seq.Join(text.DOFade(0f, duration));    // 텍스트 투명도를 0으로 페이드

        seq.AppendInterval(0.5f);   // 페이드 후 잠시 대기

        if (onComplete != null)
            seq.AppendCallback(() => onComplete.Invoke());      // 완료 콜백 실행
    }
    #endregion

    #region PopUI
    /// <summary>
    /// 팝업 UI를 스택에 추가하고 활성화하는 메서드
    /// 최초 팝업일 경우 배경을 함께 활성화
    /// </summary>
    public void OpenPopup(PopupController popup)
    {
        if (popupHistory.Count == 0)
            popup.Backdrop.SetActive(true);

        popup.PopupUI.SetActive(true);

        popupHistory.Push(popup);
    }

    /// <summary>
    /// 가장 최근의 팝업 UI를 비활성화하고 스택에서 제거하는 메서드
    /// 마지막 팝업일 경우 배경도 비활성화
    /// </summary>
    public void ClosePopup()
    {
        if (popupHistory.Count == 0) return;

        PopupController top = popupHistory.Pop();

        if (popupHistory.Count == 0)
            top.Backdrop.SetActive(false);

        top.PopupUI.SetActive(false);
    }

    /// <summary>
    /// 팝업 UI 스택을 초기화하는 메서드
    /// </summary>
    public void ClearPopupHistory() => popupHistory.Clear();
    #endregion

    #region Phase Start Text Animation
    /// <summary>
    /// 페이즈 정보를 화면 밖에서 들어와 중앙에서 관성 애니메이션 후 
    /// 다시 화면 밖으로 나가는 텍스트 연출하는 메서드
    /// </summary>
    /// <param name="text">애니메이션을 적용할 TMP_Text 객체</param>
    public void ShowPhaseStartText(TMP_Text text)
    {
        // 캔버스 기준 너비 가져오기
        float canvasWidth = ((RectTransform)text.rectTransform.parent).rect.width;

        // 시작 위치: 화면 왼쪽 바깥
        text.rectTransform.anchoredPosition =
            new Vector2(-canvasWidth, text.rectTransform.anchoredPosition.y);

        // 스케일 초기화
        text.rectTransform.localScale = Vector3.one;
        if (!text.gameObject.activeSelf)
            text.gameObject.SetActive(true);

        // DOTween 시퀀스
        Sequence seq = DOTween.Sequence();

        // 달려오기 (왼쪽 밖 → 중앙)
        seq.Append(text.rectTransform.DOAnchorPosX(0, 1.0f).SetEase(Ease.OutExpo));

        // 관성 변형 (대각선 늘어남 후 복원)
        seq.Append(text.rectTransform.DOScale(new Vector3(1.2f, 0.8f, 1f), 0.25f)
            .SetLoops(2, LoopType.Yoyo)
            .SetEase(Ease.OutQuad));

        // 3) 잠깐 멈추는 연출 (optional)
        seq.AppendInterval(0.5f);

        // 4) 다시 오른쪽 화면 밖으로 슝 나가기
        seq.Append(text.rectTransform.DOAnchorPosX(canvasWidth, 0.8f).SetEase(Ease.InBack));
    }
    #endregion

    #region Damage Text
    /// <summary>
    /// 데미지 텍스트 풀에서 객체를 요청하는 메서드
    /// 풀 생성 여부를 확인하고, 존재하지 않으면 새로 생성
    /// </summary>
    /// <param name="target">데미지 텍스트를 표시할 대상 Transform</param>
    /// <returns>풀에서 가져온 DmgFloatPooledObject 객체</returns>
    public DmgFloatPooledObject RequestDamageText(Transform target)
    {
        // 데미지 텍스트 풀 존재 여부를 확인
        if (!isDmgTextPoolExist)
        {
            // 풀 생성 (프리팹, 초기 사이즈, 최대 용량, 초과 시 재사용 여부)
            GameModeManager.PoolManager.CreatePool(dmgTextObj, size, capacity, true);

            // 풀 생성 상태 플래그 활성화
            isDmgTextPoolExist = true;
        }

        // 풀에서 데미지 텍스트 객체를 가져옴
        // 위치는 대상의 현재 위치, 회전은 기본값(Quaternion.identity)
        return GameModeManager.PoolManager.GetPool(
            dmgTextObj,
            target.position,
            Quaternion.identity
        ) as DmgFloatPooledObject;
    }

    /// <summary>
    /// 데미지 텍스트 풀 존재 여부 플래그를 초기화합니다.
    /// </summary>
    public void ResetDmgTextPoolExist() => isDmgTextPoolExist = false;
    #endregion

    #region Enemy Hp Bar
    /// <summary>
    /// * 작성자: 공민기
    ///  - enemyController에서 호출.
    ///  - 적 체력바 프리팹의 풀을 만들어서 풀 오브젝트를 리턴한다.
    /// </summary>
    public EnemyHealthBarPooledObj RequestEnemyHealthBar(Transform target)
    {
        if(!isEnemyHealthBarPoolExist)
        {
            GameModeManager.PoolManager.CreatePool(enemyHealthBarPooledObj, initialHealBarCnt, maxHealthBarCnt, true);
            isEnemyHealthBarPoolExist = true;
        }

        return GameModeManager.PoolManager.GetPool(
            enemyHealthBarPooledObj,                    // 가져올 풀 오브젝트 종류
            target.position,                            // 적 위치 위치
            Quaternion.identity
        ) as EnemyHealthBarPooledObj;
    }
    #endregion
}