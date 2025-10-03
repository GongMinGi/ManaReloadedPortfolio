using UnityEngine;
using AK.Wwise;

/// <summary>
/// This MonoBehaviour script controls Wwise events for a ParticleSystem.
/// It plays a sound event when the particle system starts playing and another when it stops.
/// This script is designed for ParticleSystem components without a separate Renderer component.
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class WwiseParticleSoundController : MonoBehaviour
{
    [Tooltip("Wwise Event to post when the Particle System starts playing.")]
    public AK.Wwise.Event onPlayEvent;

    [Tooltip("Wwise Event to post when the Particle System stops playing. Leave empty for no sound.")]
    public AK.Wwise.Event onStopEvent;

    private ParticleSystem particleSystem;
    private bool wasPlaying = false;

    private void Awake()
    {
        particleSystem = GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        if (particleSystem == null)
        {
            return;
        }

        bool isCurrentlyPlaying = particleSystem.isPlaying;

        // 파티클 시스템이 재생을 시작했을 때 (이전에는 멈춰있었음)
        if (isCurrentlyPlaying && !wasPlaying)
        {
            if (onPlayEvent != null)
            {
                onPlayEvent.Post(gameObject);
            }
        }
        // 파티클 시스템이 재생을 멈췄을 때 (이전에는 재생 중이었음)
        else if (!isCurrentlyPlaying && wasPlaying)
        {
            if (onStopEvent != null)
            {
                onStopEvent.Post(gameObject);
            }
        }

        // 현재 상태를 다음 프레임을 위해 저장
        wasPlaying = isCurrentlyPlaying;
    }
}