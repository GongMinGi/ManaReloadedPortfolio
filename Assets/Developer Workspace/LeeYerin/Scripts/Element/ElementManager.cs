using FullOpaqueVFX;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 개발자: 이예린
/// 
/// 플레이어가 선택한 원소를 키 입력(W, A, S, D)에 바인딩하고,
/// UI 슬롯 및 플레이어 컨트롤러에 해당 정보를 반영하는 매니저 클래스
/// 
/// 싱글톤(Singleton) 패턴을 기반으로 하며,
/// 씬 전환 시에도 파괴되지 않고 유지
/// </summary>
public class ElementManager : MonoBehaviour
{
    private ElementManager instance;

    [Tooltip("Element types currently bound to each key (W, A, S, D). Index mapping: 0 = W, 1 = A, 2 = S, 3 = D.")]
    [SerializeField] E_CastingType [] boundElements = new E_CastingType[4];

    [SerializeField] private ElementSlot targetElementSlot;

    public ElementSlot TargetElementSlot 
    { 
        get { return targetElementSlot; } 
        set 
        {
            if (targetElementSlot == value)
            {
                return;
            }

            UpdateTargetSlotColor(value);
            targetElementSlot = value;
        } 
    }

    #region Unity Event
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            GameModeManager.ElementManager = instance;
            DontDestroyOnLoad(gameObject);  // 씬 전환 시 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region Element Binding System
    /// <summary>
    /// 특정 원소 키 슬롯에 새로운 원소 타입을 바인딩하는 메서드
    /// </summary>
    /// <param name="type">바인딩할 원소 타입</param>
    /// <returns>바인딩 성공 시 true, 실패 시 false</returns>
    public bool SetElementBinding(E_CastingType type)
    {
        int index = GetIndex(targetElementSlot.Key);

        if (index == -1)
        {
            return false;   // 유효하지 않은 키
        }

        boundElements[index] = type;

        // 해당 원소 슬롯의 UI 반영
        UpdateElementSlots(targetElementSlot.Key, targetElementSlot.Slot);

        return true;
    }

    /// <summary>
    /// 현재 바인딩된 원소 정보를 플레이어 컨트롤러에 반영하는 메서드
    /// </summary>
    /// <param name="player">플레이어 컨트롤러</param>
    public void ApplyElementBindingsToPlayer(PlayerController player)
    {
        for (int i = 0; i < boundElements.Length; i++)
        {
            player.SetCastingKeyBinding(GetKey(i), boundElements[i]);
        }
    }
    #endregion

    #region UI Synchronization
    /// <summary>
    /// 지정된 키 슬롯의 UI 이미지를 현재 바인딩된 원소 아이콘으로 업데이트하는 메서드
    /// </summary>
    /// <param name="key">업데이트할 키</param>
    /// <param name="slot">해당 키에 대응하는 원소 슬롯 이미지</param>
    public void UpdateElementSlots(Key key, Image slot)
    {
        int index = GetIndex(key);

        if (index == -1)
        {
            Debug.LogWarning("올바르지 않은 키 세팅을 가진 원소 슬롯에 대한 세팅 시도입니다. 원소 슬롯에 할당된 키 세팅을 확인해주세요.");
            return;
        }

        slot.sprite = GameModeManager.UIManager.GetElementSprite(boundElements[GetIndex(key)]);
    }
    #endregion

    #region Key <-> Index Mapping 
    /// <summary>
    /// 키를 해당 인덱스(0~3)로 변환하는 메서드
    /// </summary>
    /// <param name="key">변환할 키</param>
    /// <returns>키에 대응하는 인덱스, 유효하지 않으면 -1</returns>
    private int GetIndex(Key key)
    {
        return key switch
        {
            Key.W => 0,
            Key.A => 1,
            Key.S => 2,
            Key.D => 3,
            _ => -1
        };
    }

    /// <summary>
    /// 인덱스를 해당 키(W, A, S, D)로 변환하는 메서드
    /// </summary>
    /// <param name="index">변환할 인덱스</param>
    /// <returns>인덱스에 대응하는 키, 유효하지 않으면 Key.None</returns>
    private Key GetKey(int index)
    {
        return index switch
        {
            0 => Key.W,
            1 => Key.A,
            2 => Key.S,
            3 => Key.D,
            _ => Key.None
        };
    }
    #endregion

    /// <summary>
    /// 현재 플레이어가 소유한 원소 기반으로 스킬 사용 가능 여부를 판단하는 메서드
    /// 
    /// 조합 마법 스킬의 레시피(CastList)에 있는 모든 원소가
    /// 플레이어의 boundElements에 포함되어 있는지 검사
    /// </summary>
    /// <param name="skill">검사할 스킬</param>
    /// <returns>필요 원소를 모두 갖추면 true, 아니면 false</returns>
    public bool HasElementsForSkill(SkillHashTable skill)
    {
        // 중복 제거를 위해 HashSet 생성
        HashSet<E_CastingType> recipeSet = new(skill.CastList);

        // 각 원소가 플레이어 소유 원소에 있는지 확인
        foreach (var element in recipeSet)
        {
            if (boundElements.Contains(element) == false)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 플레이어가 설정 중인 원소 슬롯의 시각적 상태를 갱신하는 메서드
    /// </summary>
    /// <param name="newTargetSlot">새로 지정될 타깃 슬롯</param>
    private void UpdateTargetSlotColor(ElementSlot newTargetSlot)
    {
        // 이전 슬롯의 색상을 기본값으로 되돌림
        if (targetElementSlot != null)
        {
            targetElementSlot.Slot.color = Color.white;
        }

        // 플레이어가 설정 중인 슬롯을 시각적으로 표시
        if (newTargetSlot != null)
        {
            newTargetSlot.Slot.color = Color.gray;
        }
    }
}
