using System.Collections;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.UI;

/// <summary>
/// * 작성자 : 공민기
///   - 캐스팅한 원소에 전기 속성이 포함되어 잇는 경우 시전되는 원거리 공격
///   - 마우스 좌클릭을 누르는 동안 충전하고 때는 순간에 마법을 발사한다
///   
/// </summary>
public class RangedChargeConeAttack : MonoBehaviour, IRangedAttack
{

    [Header("Cone Parameters")]
    [SerializeField] private float radius = 6f;
    [SerializeField] private float angle = 60f;
    [SerializeField] private bool flatcone = true;

    [Header("Charge Parameter")]
    [SerializeField] private int baseDamage = 12;
    [SerializeField] private int damageStep = 8;
    [SerializeField] private int maxDamage = 60;
    [SerializeField] private float chargeInterval = 0.5f;


    [Header("Layers")]
    [SerializeField] private LayerMask enemyLayer;

    float cosThreshold;
    int currentDamage;
    bool isCharging;
    Coroutine chargeRoutine;


    private void Awake() => cosThreshold = Mathf.Cos(angle * 0.5f *  Mathf.Deg2Rad);

    public void ExecuteAttack(E_CastingType type)
    {
        if (isCharging) return;
        Debug.Log("차지시작");
        chargeRoutine = StartCoroutine(ChargeProcess());
        
    }

    public void Stop()
    {
        if(!isCharging) return;
        StopCoroutine(chargeRoutine);
        isCharging = false;

    }


    IEnumerator ChargeProcess()
    {
        isCharging = true;
        currentDamage = baseDamage;
        float elapsed = 0f;

        while (Mouse.current.leftButton.isPressed)
        {
            elapsed += Time.deltaTime;

            if(elapsed >= chargeInterval)
            {
                elapsed -= chargeInterval;
                currentDamage = Mathf.Min(currentDamage + damageStep, maxDamage);
                Debug.Log("차지단계 증가");
                //추후 차지 단계별 사운드, vfx 업데이트
            }

            yield return null;
        }


        FireConeDamage();
        isCharging = false;
    }


    void FireConeDamage()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius, enemyLayer);
        Vector3 forward = transform.forward;
        if (flatcone) forward.y = 0;
        forward.Normalize();

        Debug.Log("차지공격 발사");

        foreach (var hit in hits)
        {
            Vector3 dir = hit.transform.position - transform.position;
            if (flatcone) dir.y = 0;
            dir.Normalize();

            if (Vector3.Dot(forward, dir) >= cosThreshold)
            {
                Debug.Log("데미지가 적용되었습니다.");
                // 데미지 적용
            }
        }

        // 발사 파티클 추가 적용 필요
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.25f);
        UnityEditor.Handles.color = Gizmos.color;
        UnityEditor.Handles.DrawSolidArc(
            transform.position,
            flatcone ? Vector3.up : transform.up,
            Quaternion.Euler(0f, -angle * 0.5f, 0f) * transform.forward,
            angle,
            radius
            );
    }

}
