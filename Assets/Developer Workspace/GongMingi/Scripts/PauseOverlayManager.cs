using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// 개발자 : 공민기
/// - ESC로 일시정지/재개 + "메인으로" 이동을 처리하는 공용 오버레이 컨트롤러.
/// - InputActionReference를 통해서 PlayerInput의 인풋정보와 연결해서 사용
/// </summary>
public class PauseOverlayManager : MonoBehaviour
{
    private PauseOverlayManager instance;

    [SerializeField] InputActionReference escAction;
    [SerializeField] GameObject pauseUI;

    private bool isPaused;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            GameModeManager.PauseOverlayManager = instance;
        }

        else
        {
            Destroy(this.gameObject);
        }

        if(Time.timeScale == 0f)                            // 이전 씬에서 timescale이 0인체로 남았던 경우 복구해준다.
        {
            Time.timeScale = 1f;
        }

        if(pauseUI != null)
        {
            pauseUI.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (escAction != null)
        {
            escAction.action.performed += OnEscPerformed;       // esc가 눌렸을때 사용될 콜백 함수 등록
            escAction.action.Enable();                          // 인풋 액션을 실제로 활성화 시키는 코드 (esc 입력 감지 시작)
        }
    }

    private void OnDisable()
    {
        if(escAction != null)
        {
            escAction.action.performed -= OnEscPerformed;
            escAction.action.Disable();
        }

        if(isPaused == true)                                    // 비활성화 상태에서 씬 언로드 시 타임스케일 복구
        {
            Time.timeScale = 1f;
        }
    }

    void OnEscPerformed(InputAction.CallbackContext ctx)
    {
        if (isPaused == false)
        {
            PauseGame();
        }

        else
        {
            ResumeGame();
        }
    }

    /// <summary>
    /// - 인게임에서 esc를 눌렀을 때 실행돼는 메서드
    /// - 시간을 멈추고, pause ui를 활성화시킨다. 
    /// </summary>
    public void PauseGame()
    {
        if( isPaused == true)
        {
            return;
        }

        isPaused = true;
        Time.timeScale = 0f;

        if (pauseUI != null)
        {
            pauseUI.SetActive(true);
        }
    }

    /// <summary>
    /// - paused ui에서 버튼을 눌렀을때 실행돼는 메서드
    /// - 시간을 다시 흐르게 하고 ui를 비활성화 시킨다.
    /// </summary>
    public void ResumeGame()
    {
        if (isPaused == false)
        {
            return;
        }

        isPaused = false;
        Time.timeScale = 1f; 

        if(pauseUI != null)
        {
            pauseUI.SetActive(false);
        }
    }

    /// <summary>
    /// UI 버튼: 계속 하기
    /// - paused ui에서 버튼을 눌렀을때 실행돼는 메서드
    /// - 시간을 다시 흐르게 하고 ui를 비활성화 시킨다.
    /// </summary>
    public void OnClickResume() => ResumeGame();

    /// <summary>
    /// UI 버튼: 메인으로
    /// - 페이드 아웃 후 게임 메인 메뉴 화면으로 이동하는 메서드
    /// </summary>
    public void OnClickGoToMain()
    {
        ResumeGame();

        GameModeManager.UIManager.ResetDmgTextPoolExist();

        GameModeManager.UIManager.LoadIntoLoadoutUI = false;
        GameModeManager.UIManager.FadeOut(() =>
        {
            SceneManager.LoadScene("Main Menu & Loadout Scene");
        });
    }

    /// <summary>
    /// 게임을 종료하는 메서드
    /// - 플랫폼에 맞는 종료 처리 수행
    /// </summary>
    public void OnClickExit()
    {
        ResumeGame();
        GameModeManager.ExitGame();
    }



}
