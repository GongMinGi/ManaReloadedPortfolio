using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 개발자: 이예린, 공민기
/// 
/// 전 게임 메뉴 흐름을 관리하는 매니저 클래스
/// 게임 시작 전 로드아웃 UI 전환, 게임 씬 이동, 게임 종료 기능을 포함되어 있음
/// </summary>
public class PreGameFlowManager : MonoBehaviour
{
    [Header("UI Setting")]
    [SerializeField] GameObject gameMenuUI;           // 게임 메뉴 UI
    [SerializeField] GameObject loadOutUI;            // 로드 아웃 UI
    [SerializeField] GameObject elementListUI;        // 원소 선택 UI
    [SerializeField] GameObject itemSlotListUI;       // 아이템 슬롯 UI
    [SerializeField] GameObject notImplementedUI;     // 미구현 표시 이미지 UI
    [SerializeField] Animator playerAnimator;         // 로드아웃의 플레이어 애니메이터

    [Tooltip("Game scene name string")]
    [SerializeField] string gameSceneName = "Game Scene";
    [SerializeField] string qaSceneName = "QA Scene";

    private ElementSlot selectedSlot;                   // 속성 교체를 위해 현재 선택된 원소 슬롯

    #region Unity Event
    private void Start()
    {
        if (GameModeManager.UIManager.IsFirstLaunch)    // 게임 실행 후 첫 진입일 경우
        {
            return;
        }

        GameModeManager.UIManager.ClearPopupHistory();      // UIManager의 ClearPopupHistory 스택 초기화

        if (GameModeManager.UIManager.LoadIntoLoadoutUI)    // 게임 로드아웃으로 이동일 경우
        {
            gameMenuUI.SetActive(false);
            loadOutUI.SetActive(true);
            GameModeManager.UIManager.FadeIn();
        }
        else
        {
            // 게임 메인 메뉴로 이동일 경우
            GameModeManager.UIManager.FadeIn();
        }
    }
    #endregion

    /// <summary>
    /// 바꿀 속성을 골랐을때 ( a <= b 로 바꿀때 b를 눌렀을때) 호출되는 메서드
    /// 실제 elemental slot의 값들을 변경한다.
    /// </summary>
    public void ChangeToSelectedElement(ElementSlot elementToChange)
    {
        if(selectedSlot == null)                                    // 선택된 슬롯이 없는 경우 리턴
        {
            return;
        }

        selectedSlot.SetElementToThisSlot(elementToChange.curType);  // 바꿀속성으로 슬롯이 들고 있는 속성 변경
        GameModeManager.SetElementBinding(selectedSlot.curKey, selectedSlot.curType);

        selectedSlot = null;
    }

    /// <summary>
    /// 장비 슬롯 창을 비활성화하고 원소 선택 창을띄우는 메서드
    /// 전달 받은 슬롯을 선택한 슬롯에 저장
    /// </summary>
    public void OpenElementList(ElementSlot selectedSlotParam)
    {
        selectedSlot = selectedSlotParam;                       // 전달 받은 슬롯을 선택한 슬롯에 저장

        itemSlotListUI.SetActive(false);                        // 속성 변경창을 띄우고 인벤토리 비활성하ㅗ
        notImplementedUI.SetActive(false);
        elementListUI.SetActive(true);
    }

    /// <summary>
    /// 게임 메뉴에서 로드아웃 UI로 전환하는 메서드
    /// 페이드 아웃 후 메뉴 UI 비활성화, 로드아웃 UI 활성화, 페이드 인 진행
    /// </summary>
    public void OpenLoadOut()
    {
        GameModeManager.UIManager.FadeOut(() => 
        {
            gameMenuUI.SetActive(false);
            loadOutUI.SetActive(true);
            GameModeManager.UIManager.FadeIn();
        });
    }

    /// <summary>
    /// 로드아웃UI의 게임 시작 버튼과 연결되는 메서드로, 씬을 게임 씬으로 전환함
    /// 페이드 아웃 후 씬 로드 진행함
    /// </summary>
    public void GameStart()
    {
        //GameModeManager.SoundManager.StopBGM();      // PreGameFlow의 BGM 종료
        playerAnimator.SetTrigger("IsCompete");    // 캐릭터 출전 애니메이션 트리거

        GameModeManager.UIManager.FadeOut(() =>
        {
            SceneManager.LoadScene(gameSceneName);
        });
    }

    /// <summary>
    /// QA룸 씬으로 이동하는 메서드
    /// </summary>
    public void OpenQARoom()
    {
        GameModeManager.UIManager.FadeOut(() =>
        {
            SceneManager.LoadScene(qaSceneName);
            GameModeManager.UIManager.FadeIn();
        });
    }

    /// <summary>
    /// 게임을 종료하는 메서드
    /// 플랫폼에 맞는 종료 처리 수행
    /// </summary>
    public void ExitGame()
    {
        GameModeManager.ExitGame();  // 게임 종료
    }
}