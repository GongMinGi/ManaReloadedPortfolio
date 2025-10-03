using UnityEngine;
using AK.Wwise;

/// <summary>
/// This MonoBehaviour script controls Wwise events based on the GameObject's active state.
/// Attach this script to any GameObject with a VFX that needs sound.
/// </summary>
public class WwiseVFXSoundController : MonoBehaviour
{
    [Tooltip("Wwise Event to post when the GameObject is enabled.")]
    public AK.Wwise.Event onEnableEvent;
    
    [Tooltip("Wwise Event to post when the GameObject is disabled.")]
    public AK.Wwise.Event onDisableEvent;

    private bool isFirstActivation = true;

    private void OnEnable()
    {
        // 게임 시작 후 첫 번째 활성화 시점인지 확인합니다.
        if (isFirstActivation)
        {
            isFirstActivation = false;
        }
        else
        {
            // 첫 번째 활성화 이후에만 사운드를 재생합니다.
            if (onEnableEvent != null)
            {
                onEnableEvent.Post(gameObject);
            }
        }
    }

    private void OnDisable()
    {
        // GameObject가 비활성화될 때 Wwise 이벤트를 재생합니다.
        if (onDisableEvent != null)
        {
            onDisableEvent.Post(gameObject);
        }
    }
}