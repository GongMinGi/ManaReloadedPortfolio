using Cinemachine;
using DG.Tweening;
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

    #region Day & Night
    [Header("Day & Night")]
    [SerializeField] GameObject dayMap;
    [SerializeField] CinemachineVirtualCamera dayCamera;
    [SerializeField] GameObject nightMap;
    [SerializeField] CinemachineVirtualCamera nightrCamera;

    [SerializeField] DayNightState dayNightState = DayNightState.Night;
    private Vector3 resetPos;
    #endregion

    #region Phase Info
    [SerializeField] int totalPhases = 0;   // 게임 전체 페이즈 수
    [SerializeField] int currentPhase = 0;  // 현재 진행 중인 페이즈 번호
    [SerializeField] TMP_Text phaseText;    // 페이즈 텍스트
    [SerializeField] float phaseTextOffsetX = 322f;
    #endregion

    #region Timer Info
    [SerializeField] TMP_Text timeText;
    private int time;   // 경과한 시간
    public int Timer => time;
    Coroutine timer;            // 게임 시간 추적용 코루틴 핸들
    #endregion

    #region Game Over Setting
    [SerializeField] GameObject gameOverUI;     // 게임 오버 UI 오브젝트
    [SerializeField] GameObject pauseGameUI;    // 게임 정지 UI 오브젝트
    [SerializeField] TMP_Text totalPlayTimeText;    // 총 게임 진행 시간 텍스트
    public bool IsGameOver { get; set; } = false;
    #endregion

    #region State
    bool isFinish = false;  // 게임 종료 여부
    bool isPaused = false;
    #endregion

    #region Initialization
    /// <summary>
    /// 전체 페이즈 수를 초기화함
    /// </summary>
    public void Initialization(int totalPhases) => this.totalPhases = totalPhases;
    #endregion

    #region Unity Event
    private void Start()
    {
        if (instance == null)
        {
            instance = this;
            GameModeManager.GameLogicManager = instance;
        }
        else
        {
            Destroy(gameObject);
        }

        if (GameModeManager.Player != null)
        {
            resetPos = GameModeManager.Player.transform.position;
        }

        // 낮/밤에 맞는 맵 오브젝트 활성화
        if (dayNightState == DayNightState.Day)
        {
            dayMap.SetActive(true);
            nightMap.SetActive(false);

            dayCamera.Priority = 10;
            nightrCamera.Priority = 0;
        }
        else
        {
            dayMap.SetActive(false);
            nightMap.SetActive(true);

            dayCamera.Priority = 0;
            nightrCamera.Priority = 10;
        }

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
        if (dayNightState == DayNightState.Day)
        {
            GameModeManager.UIManager.FadeIn(() =>
            {
                GameModeManager.UIManager.ClearPopupHistory();  // UIManager의 PopupHistory 스택 초기화
                ProceedPhase();
                timer = StartCoroutine(TrackGameTime());
            });
        }
        else
        {
            GameModeManager.UIManager.FadeIn();
            // 밤 로직
            // 
        }
    }

    /// <summary>
    /// 게임 종료 로직을 실행하는 메서드
    /// </summary>
    public void GameOver()
    {
        IsGameOver = true;
        StopCoroutine(timer);   // 타이머 종류
        totalPlayTimeText.text = $"{time / 60:D2} : {time % 60:D2}";    // 총 플레이 시간 텍스트 설정

        GameModeManager.UIManager.ResetDmgTextPoolExist();      // 데미지 텍스트 풀 존재 여부 플래그를 초기화

        Sequence gameOverUISequence = DOTween.Sequence();

        gameOverUISequence.AppendInterval(1.2f);    // 플레이어 사망 애니메이션만큼 시간차를 둔 후

        gameOverUISequence.AppendCallback(() => 
        {
            gameOverUI.SetActive(true);     // 게임 오버 UI 활성화
        });

    }

    [ContextMenu("ChageDayNightState")]
    public void ChageDayNightState()
    {
        GameModeManager.UIManager.FadeOut(() =>
        {
            dayCamera.Priority = 0;
            nightrCamera.Priority = 10;

            GameModeManager.Player.transform.position = resetPos;

            if (dayNightState == DayNightState.Day)
            {
                dayMap.SetActive(false);
                nightMap.SetActive(true);

                GameModeManager.UIManager.FadeIn(() =>
                {

                });

                dayNightState = DayNightState.Night;
            }
            else
            {
                dayCamera.Priority = 10;
                nightrCamera.Priority = 0;

                dayMap.SetActive(true);
                nightMap.SetActive(false);

                GameModeManager.UIManager.FadeIn(() =>
                {
                    ProceedPhase();
                });

                dayNightState = DayNightState.Day;
            }
        });
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
        {
            if (dayNightState == DayNightState.Day)
            {
                GameModeManager.EnemyManager.StartSpawnEnemyLoop(currentPhase++);
                phaseText.text = $"{currentPhase} Phase";   // 페이즈 정보 텍스트 업데이트
                GameModeManager.UIManager.ShowPhaseStartText(phaseText, phaseTextOffsetX);    // 페이즈 테스트 애니메이션 실행
            }
            else
            {

            }

        }
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
        GameModeManager.PauseOverlayManager.ResumeGame();

        GameModeManager.UIManager.ResetDmgTextPoolExist();

        GameModeManager.UIManager.FadeOut(() =>
        {
            GameModeManager.UIManager.ClearPopupHistory();      // UIManager의 ClearPopupHistory 스택 초기화
            SceneManager.LoadScene("Game Scene");
        });
    }

    /// <summary>
    /// 페이드 아웃 후 로드아웃 화면으로 이동하는 메서드
    /// </summary>
    public void GoToLoadout()
    {
        GameModeManager.PauseOverlayManager.ResumeGame();

        GameModeManager.UIManager.ResetDmgTextPoolExist();

        GameModeManager.UIManager.LoadIntoLoadoutUI = true;
        GameModeManager.UIManager.FadeOut(() =>
        {
            SceneManager.LoadScene("Main Menu & Loadout Scene");
        });
    }
    #endregion
}

public enum DayNightState
{
    Day,
    Night
}