using UnityEngine;
using UnityEngine.InputSystem;



/// <summary>
/// 그냥 gpt ㅈㄴ 복붙함 내일고쳐야함ㅠ
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
        // 1) 입력: 왼/오른 Ctrl 모두 허용
        bool ctrl =
            (Keyboard.current?.leftCtrlKey.isPressed ?? false) ||
            (Keyboard.current?.rightCtrlKey.isPressed ?? false);

        // 2) 상태 변할 때만 토글
        if (ctrl != isShown) SetVisible(ctrl);

        // 3) 위치만 따라가고 회전은 고정
        if (isShown && player != null && castingCircleRoot != null)
        {
            Vector3 p = player.position;
            p.y += yOffset;
            castingCircleRoot.transform.position = p;

            if (keepWorldRotation)
                castingCircleRoot.transform.rotation = Quaternion.identity; // 월드 회전 고정
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
