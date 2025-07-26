using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 개발자: 이예린
/// 
/// 게임 로직의 전체 흐름을 제어하는 매니저 클래스
/// 페이즈 진행, 타이머 추적, 초기화 등을 담당
/// </summary>
public class GameLogicManager : MonoBehaviour
{
    #region Singleton
    private GameLogicManager instance;
    #endregion

    #region Phase Info
    [SerializeField] int totalPhases = 0;   // 게임 전체 페이즈 수
    [SerializeField] int currentPhase = 0;  // 현재 진행 중인 페이즈 번호
    #endregion

    #region Timer Info
    [SerializeField] TMP_Text timeText;
    private int time;   // 경과한 시간
    public int Time => time;
    Coroutine timer;            // 게임 시간 추적용 코루틴 핸들
    #endregion

    #region Game Over UI Setting
    [SerializeField] GameObject gameOverUI;     // 게임 오버 UI 오브젝트
    [SerializeField] TMP_Text totalPlayTimeText;    // 총 게임 진행 시간 텍스트
    #endregion

    #region State
    bool isFinish = false;  // 게임 종료 여부
    #endregion

    #region Initialization
    /// <summary>
    /// 전체 페이즈 수를 초기화함
    /// </summary>
    public void Initialization(int totalPhases) => this.totalPhases = totalPhases;
    #endregion

    #region Unity Event
    private IEnumerator Start()
    {
        if (instance == null)
        {
            instance = this;
            GameModeManager.GameLogicManager = instance;
        }
        else
            Destroy(gameObject);

        // GameModeManager 세팅 완료 및 totalPhases 설정 대기
        yield return new WaitUntil(() => totalPhases != 0  && GameModeManager.IsReady);

        StartGame();
    }
    #endregion

    #region Game Control
    /// <summary>
    /// 게임 시작 시 호출되는 메서드
    /// 첫 페이즈를 진행하고 타이머 시작함
    /// </summary>
    private void StartGame()
    {
        // FadeIn 후 게임 로직 실행
        GameModeManager.UIManager.FadeIn(() => 
        {
            GameModeManager.UIManager.ClearPopupHistory();  // UIManager의 PopupHistory 스택 초기화
            ProceedPhase();
            timer = StartCoroutine(TrackGameTime());
        });
    }

    /// <summary>
    /// 게임 종료 로직을 실행하는 메서드
    /// </summary>
    public void GameOver()
    {
        StopCoroutine(timer);   // 타이머 종류
        totalPlayTimeText.text = $"{time / 60:D2} : {time % 60:D2}";    // 총 플레이 시간 텍스트 설정
        gameOverUI.SetActive(true);     // 게임 오버 UI 활성화
    }
    #endregion

    #region Phase Logic
    /// <summary>
    /// 현재 페이즈를 진행하고, 전체 페이즈가 종료되었는지 여부를 판단하여 다음 단계로 진행하는 메서드
    /// 종료가 아닐 시 다음 페이즈를 진행,
    /// 종료 시 게임 종료 로직을 실행함
    /// </summary>
    public void ProceedPhase()
    {
        if (currentPhase < totalPhases)
            GameModeManager.EnemyManager.StartSpawnEnemyLoop(currentPhase++);
        else
        {
            Debug.Log("게임 페이즈 로직이 전부 종료되었습니다.");
            GameOver();     // 게임 종료 로직 실행
        }
    }
    #endregion

    #region Check Time
    private IEnumerator TrackGameTime()
    {
        time = 0;

        while (!isFinish)
        {
            yield return new WaitForSeconds(1f);

            timeText.text = $"{time / 60:D2} : {time % 60:D2}";
            time++;
        }
    }
    #endregion

    #region After Game Over
    /// <summary>
    /// 페이드 아웃 후 게임을 다시 시작하는 메서드
    /// </summary>
    public void GameRetry()
    {
        GameModeManager.UIManager.FadeOut(() =>
        {
            GameModeManager.UIManager.ClearPopupHistory();      // UIManager의 ClearPopupHistory 스택 초기화
            SceneManager.LoadScene("Game Scene");
        });
    }

    /// <summary>
    /// 페이드 아웃 후 게임 메인 메뉴 화면으로 이동하는 메서드
    /// </summary>
    public void GoToMainMenu()
    {
        GameModeManager.UIManager.LoadIntoLoadoutUI = false;
        GameModeManager.UIManager.FadeOut(() => 
        { 
            SceneManager.LoadScene("Main Menu & Loadout Scene");
        });
    }

    /// <summary>
    /// 페이드 아웃 후 로드아웃 화면으로 이동하는 메서드
    /// </summary>
    public void GoToLoadout()
    {
        GameModeManager.UIManager.LoadIntoLoadoutUI = true;
        GameModeManager.UIManager.FadeOut(() =>
        {
            SceneManager.LoadScene("Main Menu & Loadout Scene");
        });
    }

    /// <summary>
    /// 게임을 종료하는 메서드
    /// 플랫폼에 맞는 종료 처리 수행
    /// </summary>
    public void ExitGame()
    {
        GameModeManager.ExitGame();
    }
    #endregion
}