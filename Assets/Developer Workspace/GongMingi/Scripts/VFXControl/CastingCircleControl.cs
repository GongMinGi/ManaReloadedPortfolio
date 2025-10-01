using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// * 개발자:공민기
///  - 마법 캐스팅 중에 마법진이 플레이어밑에서 돌아가도록 수정
///  - 플레이어 회전값에 상관없이 돌아간다
/// </summary>
public class CastingCircleControl : MonoBehaviour
{
    [SerializeField] GameObject castingCircleRoot;   // 루트 오브젝트(비/활성)
    [SerializeField] ParticleSystem castingCircleVFX;// 파티클(선택)

    [SerializeField] Transform player;               // 따라갈 대상
    [SerializeField] float yOffset = 0.02f;          // 바닥 깜빡임 방지
    [SerializeField] bool keepWorldRotation = true;  // 플레이어 회전 무시

    bool isShown;

    void Awake()
    {
        // 시작 시 꺼두기(Stop Action은 None 권장)
        SetVisible(false);
    }

    void LateUpdate()
    {
        // 1) 입력: space가 입력되어있을때만,
        bool spacepressed = Keyboard.current.spaceKey.isPressed;

        // 2) 상태 변할 때만 토글
        if (spacepressed != isShown) SetVisible(spacepressed);

        // 3) 위치만 따라가고 회전은 고정
        if (isShown && player != null && castingCircleRoot != null)
        {
            Vector3 p = player.position;
            p.y += yOffset;
            castingCircleRoot.transform.position = p;

            if (keepWorldRotation)
            {
                transform.rotation = Quaternion.identity; // 월드 회전 고정
            }
        }
    }

    void SetVisible(bool on)
    {
        isShown = on;

        if (castingCircleRoot != null)
            castingCircleRoot.SetActive(on);

        if (castingCircleVFX != null)
        {
            if (on) castingCircleVFX.Play(true);
            else castingCircleVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}
