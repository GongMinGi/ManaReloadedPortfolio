using UnityEngine;

/// <summary>
/// 개발자: 이예린
/// 
/// 특정 행동(예: 원거리 공격) 시 재생되는 파티클 VFX를 관리하는 클래스
/// Unity의 ParticleSystem을 제어하여 VFX를 재생하거나 정지시킴
/// </summary>
public class VFXObject : MonoBehaviour
{
    [SerializeField] ParticleSystem rangedAttackVFX;    // VFX

    /// <summary>
    /// 파티클 VFX를 재생합니다.
    /// </summary>
    public void Play() => rangedAttackVFX.Play();

    /// <summary>
    /// 파티클 VFX를 정지시킵니다.
    /// </summary>
    public void Stop() => rangedAttackVFX.Stop();
}