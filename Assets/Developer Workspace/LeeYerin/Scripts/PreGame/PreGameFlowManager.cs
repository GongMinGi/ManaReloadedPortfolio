using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 개발자: 이예린
/// 
/// 전 게임 메뉴 흐름을 관리하는 매니저 클래스
/// 게임 시작 전 로드아웃 UI 전환, 게임 씬 이동, 게임 종료 기능을 포함되어 있음
/// </summary>
public class PreGameFlowManager : MonoBehaviour
{
    [Header("UI Setting")]
    [SerializeField] PopupController controlGuideUI; // 조작 법 설명 UI
    [SerializeField] GameObject gameMenuUI;     // 게임 메뉴 UI
    [SerializeField] GameObject loadOutUI;      // 로드아웃 UI
    [SerializeField] Animator playerAnimator;   // 로드아웃의 플레이어 애니메이터

    [Tooltip("Game scene name string")]
    [SerializeField] string gameSceneName = "Game Scene";
    [SerializeField] string qaSceneName = "QA Scene";

    #region Unity Event
    private IEnumerator Start()
    {
        yield return new WaitUntil(() => GameModeManager.UIManager != null && GameModeManager.SoundManager != null);

        //GameModeManager.SoundManager.PlayBGM(bgmClip.title);    // PreGameFlow의 BGM 실행

        if (GameModeManager.UIManager.IsFirstLaunch)    // 게임 실행 후 첫 진입일 경우
        {
            GameModeManager.UIManager.OpenPopup(controlGuideUI);    // 조작 법 설명 UI를 팝업 스택에 Push
            yield break;
        }

        controlGuideUI.Backdrop.SetActive(false);   //  // 조작 법 설명 UI 비활성화

        GameModeManager.UIManager.ClearPopupHistory();      // UIManager의 ClearPopupHistory 스택 초기화

        if (GameModeManager.UIManager.LoadIntoLoadoutUI)    // 게임 로드아웃으로 이동일 경우
        {
            gameMenuUI.SetActive(false);
            loadOutUI.SetActive(true);
            GameModeManager.UIManager.FadeIn();
        }
        else                                               // 게임 메인 메뉴로 이동일 경우
            GameModeManager.UIManager.FadeIn();
    }
    #endregion

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