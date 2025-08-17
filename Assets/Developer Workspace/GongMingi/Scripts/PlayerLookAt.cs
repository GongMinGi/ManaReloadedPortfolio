using Game.Combat.Stats;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLookAt : MonoBehaviour
{
    #region propertyAndField
    [SerializeField] UnitStats stats;   // 플레이어의 현재 스탯을 관리하는 컴포넌트
    [SerializeField] Camera playerCamera;                                                       // 마우스 좌표를 월드로 변환할 기준 카메라
    [SerializeField] float turnSmoothTime = 0.05f;                                              // 회전 부드럽게 만드는 시간 상수 (작을수록 즉각적)
    private bool isDie = false;                                                                 // 사망 시 회전/ 로직 중단 플래그

    #endregion

    #region UnityEvent
    private void Awake()
    {
        playerCamera = Camera.main;
        stats.OnDie += OnDie;                                                                   // OnDie 이벤트 구독
    }

    private void FixedUpdate()
    {
        PlayerRotate();   
    }
    #endregion

    #region Implementation

    private void PlayerRotate()
    {
        if (isDie) return;
        Vector3 screenPos = Mouse.current.position.ReadValue();                                 // 현재 마우스 스크린 좌표 읽기
        screenPos.z = playerCamera.WorldToScreenPoint(transform.position).z;                    // 월드 변환에 쓸 깊이(Z) 설정
        Vector3 mousePos = playerCamera.ScreenToWorldPoint(screenPos);                          // 스크린 -> 월드 좌표 변환

        Vector3 rotationDir = mousePos - transform.position;                                    // 플레이어 -> 마우스 방향 벡터
        rotationDir.y = 0;

        if (rotationDir.sqrMagnitude < 0.1f) return;                                            // 너무 가까운 경우 회전하지 않음

        Quaternion targetRot = Quaternion.LookRotation(rotationDir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.fixedDeltaTime / turnSmoothTime);


    }

    public void OnDie(float temp = 0)
    {
        isDie = true;
    }

    #endregion
}
