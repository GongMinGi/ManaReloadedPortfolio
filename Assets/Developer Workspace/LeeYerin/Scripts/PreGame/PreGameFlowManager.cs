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
    [SerializeField] GameObject gameMenuUI;     // 게임 메뉴 UI
    [SerializeField] GameObject loadOutUI;      // 로드아웃 UI

    #region Unity Event
    private IEnumerator Start()
    {
        yield return new WaitUntil(() => GameModeManager.UIManager != null);

        if (GameModeManager.UIManager.IsFirstLaunch)    // 게임 실행 후 첫 진입일 경우
            yield break;

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
        GameModeManager.UIManager.FadeOut(() =>
        {
            SceneManager.LoadScene("Game Scene");
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