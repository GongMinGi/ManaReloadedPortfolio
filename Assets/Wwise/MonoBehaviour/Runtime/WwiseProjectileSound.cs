using UnityEngine;
using AK.Wwise;

/// <summary>
/// This MonoBehaviour script posts a Wwise event when the attached GameObject becomes active.
/// It also stops the event when the GameObject is deactivated.
/// </summary>
public class WwiseProjectileSound : MonoBehaviour
{
    // Wwise Event를 Inspector에서 설정하기 위한 public 변수
    [Tooltip("Wwise Event to post when this projectile becomes active.")]
    public AK.Wwise.Event onLaunchEvent;

    // GameObject가 활성화될 때마다 호출되는 Unity 내장 함수
    private void OnEnable()
    {
        // Inspector에 할당된 Wwise Event가 있다면 재생합니다.
        if (onLaunchEvent != null)
        {
            onLaunchEvent.Post(gameObject);
        }
    }

    // GameObject가 비활성화될 때마다 호출되는 Unity 내장 함수
    private void OnDisable()
    {
        // 재생 중인 사운드를 멈춥니다.
        if (onLaunchEvent != null)
        {
            onLaunchEvent.Stop(gameObject);
        }
    }
}