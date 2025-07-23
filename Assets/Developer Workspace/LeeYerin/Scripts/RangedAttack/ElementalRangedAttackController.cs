using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 개발자: 이예린
/// 
/// 플레이어가 보유한 원소 속성을 바탕으로,
/// 우선순위에 따라 적절한 원거리 공격을 자동 선택하고 실행하는 컨트롤러
/// 
/// 원소의 우선순위는 ScriptableObject(ElementPriorityData)에 정의되며,
/// 사용자는 사전에 정의된 공격 유형(Charge, Beam, Cone) 중 하나를 실행하게 됨
/// </summary>
public class ElementalRangedAttackController : MonoBehaviour
{
    [SerializeField] ElementPriorityData elementPriorityData;

    private Dictionary<E_CastingType, int> castedElementCount = new();

    /// <summary>
    /// enum으로 정의된 casting type을  리스트에 플레그 형태로 저장
    /// </summary>
    public Dictionary<E_CastingType, int> CastedElementCount => castedElementCount;


    [Header("RangedAttack Components")]
    [SerializeField] RangedChargeConeAttack chargeConeAttack;
    [SerializeField] RangedChargeProjectileAttack chargeProjectileAttack;
    [SerializeField] RangedBeamAttack beamAttack;
    [SerializeField] RangedConeAttack coneAttack;


    #region Casted Element Count Clear
    /// <summary>
    /// 보유한 원소 속성별 카운트를 초기화하는 메서드
    /// 모든 원소의 개수를 0으로 설정하여 원소 보유 정보를 리셋함
    /// </summary>
    private void ClearCastedElementCount()
    {
        foreach (var element in castedElementCount.Keys.ToList())
            castedElementCount[element] = 0;
    }
    #endregion

    #region Elemental Attack Dispatcher
    /// <summary>
    /// 현재 보유한 원소를 기반으로 적절한 원거리 공격을 자동 선택하여 실행하는 메서드
    /// 우선순위와 보유 개수를 고려하여 실행되는 공격이 결정됨
    /// </summary>
    public void TryElementalRangedAttack()
    {
        if (castedElementCount == null)
            return;

        // 우선순위와 보유 개수를 기반으로 하나의 속성을 선택
        E_CastingType? castingType = SelectPrimaryCastingElement();

        if (!castingType.HasValue)
        {
            Debug.LogWarning("사용 가능한 속성이 없습니다.");
        }
        else
        {
            Debug.Log($"[ {castingType} ] 속성이 선택됐습니다.");

            // 선택된 속성에 해당하는 공격 방식 호출
            switch (castingType.Value)
            {
                case E_CastingType.Earth:
                case E_CastingType.Thunder:
                    ChargeAttack(castingType.Value);
                    break;

                case E_CastingType.Light:
                case E_CastingType.Darkness:
                    BeamAttack(castingType.Value);
                    break;

                case E_CastingType.Fire:
                case E_CastingType.Cold:
                    ConeAttack(castingType.Value);
                    break;
            }
        }

        // 보유 중인 원소 개수에 대한 데이터 정리
        ClearCastedElementCount();
    }

    /// <summary>
    /// 현재 보유한 원소들 중 가장 높은 우선순위의 원소 속성을 선택하는 메서드
    /// 단, 동일 우선순위 내에서 여러 원소가 있을 경우 개수가 많은 쪽이 우선되어 선택됨
    /// </summary>
    /// <returns>선택된 원소 속성 (없다면 null 반환)</returns>
    E_CastingType? SelectPrimaryCastingElement()
    {
        foreach (var group in elementPriorityData.PriorityGroups)
        {
            E_CastingType? castingType = null;

            foreach (var element in group.elements)
            {
                if (!castedElementCount.ContainsKey(element))
                    continue;

                // 해당 원소를 1개 이상 보유 중인지 확인
                if (castedElementCount[element] == 0)
                    continue;

                // 동일 우선순위 그룹 내에서는 개수가 가장 많은 속성을 우선 선택
                if (castingType == null || castedElementCount[castingType.Value] < castedElementCount[element])
                    castingType = element;
            }

            // 현재 우선순위 그룹에서 보유 중인 원소가 있다면,
            // 그 원소 중 가장 많이 보유한 것을 선택해 즉시 반환
            if (castingType != null)
                return castingType;
        }

        // 사용 가능한 원소가 전혀 없는 경우
        return null;
    }
    #endregion

    #region Ranged Attack Handlers
    /// <summary>
    /// Hold & Release 방식의 차지 공격을 수행하는 메서드
    /// 땅: 폭발성 투사체 / 전기: 전방 원뿔 범위 즉시 피해
    /// </summary>
    /// <param name="castingType">실행할 속성</param>
    private void ChargeAttack(E_CastingType castingType)
    {
        switch (castingType)
        {
            case E_CastingType.Earth:
                Debug.Log("폭발하는 투사체 발사!");  // Earth 속성 전용
                chargeProjectileAttack.ExecuteAttack(castingType);
                break;
            case E_CastingType.Thunder:
                Debug.Log("원뿔 모양에 한 번에 데미지를 입히는 공격 발사!");   // Lightning 속성 전용
                chargeConeAttack.ExecuteAttack(castingType);
                break;
            default:
                Debug.LogWarning("ChargeAttack 할 수 없는 원소 속성입니다.");
                break;
        }
    }

    /// <summary>
    /// Hold 방식으로 지속되는 빔 공격을 수행하는 메서드
    /// 빛/어둠: 전방 직선 방향으로 지속 피해
    /// </summary>
    /// <param name="castingType">실행할 속성</param>
    private void BeamAttack(E_CastingType castingType)
    {
        switch (castingType)
        {
            case E_CastingType.Light:
                Debug.Log("일직선의 빛의 빔 발사!"); // Lumina 속성 전용
                beamAttack.ExecuteAttack(castingType);
                break;
            case E_CastingType.Darkness:
                Debug.Log("일직선의 어둠의 빔 발사!");    // Darkness 속성 전용
                beamAttack.ExecuteAttack(castingType);
                break;
            default:
                Debug.LogWarning("BeamAttack 할 수 없는 원소 속성입니다.");
                break;
        }
    }

    /// <summary>
    /// Hold 방식의 원뿔 모양 범위 공격을 수행합니다.
    /// 불/냉기: 지속적 범위 피해
    /// </summary>
    /// <param name="castingType">실행할 속성</param>
    private void ConeAttack(E_CastingType castingType)
    {
        switch (castingType)
        {
            case E_CastingType.Fire:
                Debug.Log("불꽃의 원뿔 범위 공격 발사!");  // Fire 속성 전용
                coneAttack.ExecuteAttack(castingType);
                break;
            case E_CastingType.Cold:
                Debug.Log("냉기의 원뿔 범위 공격 발사!");   // Frost 속성 전용
                coneAttack.ExecuteAttack(castingType);
                break;
            default:
                Debug.LogWarning("ConeAttack 할 수 없는 원소 속성입니다.");
                break;
        }
    }

    #endregion
}