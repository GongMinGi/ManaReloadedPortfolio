using System.Collections;
using UnityEngine;

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
    [SerializeField] int sec;   // 경과 초
    [SerializeField] int min;   // 경과 분
    Coroutine timer;            // 게임 시간 추적용 코루틴 핸들
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
        ProceedPhase();
        timer = StartCoroutine(TrackGameTime());
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
            StopCoroutine(timer);   // 타이머 종류
        }
    }
    #endregion

    #region Check Time
    private IEnumerator TrackGameTime()
    {
        sec = 0;
        min = 0;

        while (!isFinish)
        {
            yield return new WaitForSeconds(1f);

            if (sec == 60)
            {
                min++;
                sec = 0;
            }
            else if (sec < 60)
            {
                sec++;
            }

            Debug.Log($"{min} : {sec}");
        }
    }
    #endregion
}