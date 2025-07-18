using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class PlayerCastingInputController : MonoBehaviour
{

    [Header("Casting Settings")]
    [SerializeField] private int maxInputCount = 6;                         // 조합 길이
    [SerializeField] private Key castingCompleteKey = Key.Space;    // 캐스팅 확정 키


    [Header("Event -> UI 연결")]
    public UnityEvent<E_CastingType, int> onCastAdded;                      // (타입, index)
    public UnityEvent onCastReset;

    private readonly List<E_CastingType> currentCastingList = new();



    private static readonly Dictionary<Key, E_CastingType> castingKeyMapping = new()
    {
        {Key.W, E_CastingType.Fire },
        {Key.A, E_CastingType.Frost },
        {Key.S, E_CastingType.Lightning },
        {Key.D, E_CastingType.Earth },
    };  


    void Update()
    {
        //foreach(var mappedKey in castingKeyMapping)
        //{
        //    if(mappedKey.Key == nowCasting)
        //    {
        //        AddCasting(mappedKey.Value);
        //        break;
        //    }
        //}
    }


    private void AddCasting(E_CastingType castingType)
    {
        if (currentCastingList.Count >= maxInputCount) return;      //초과 입력 무시

        currentCastingList.Add(castingType);
        onCastAdded.Invoke(castingType, currentCastingList.Count - 1);  // unity event
    }

    private void TryCastSkill()
    {
        if (currentCastingList.Count == 0) return;

        BaseSkill skill = SkillCastingManager.Instance.GetSkill(currentCastingList);

        if (skill != null)
        {
            skill.ExecuteSkill();
        }
        else
        {
            Debug.LogError("해당 조합에 매칭되는 스킬이 없습니다.");
        }

        ResetCasting();

    }

    private void ResetCasting()
    {
        currentCastingList.Clear();
        onCastReset?.Invoke();
    }

    public void OnCastingSpell(InputAction.CallbackContext ctx)
    {
        if (!ctx.started) return;                       // 버튼을 눌렀을 때만 .. 홀드.. 땔때는 모두 리턴

        var keyControl = ctx.control as KeyControl;
        if (keyControl == null) return;                 // 게임패드, 마우스 등 키보드가 아닐 경우 리턴


        

        Key key = keyControl.keyCode;                   // 새 인풋 시스템의 키 열거형
        Debug.Log(key);

        if(castingKeyMapping.TryGetValue(key, out var castingType))
        {
            AddCasting(castingType);
        }

        

    }

    public void OnCombinationMagicAttack(InputAction.CallbackContext ctx)
    {
        if (!ctx.started) return;
        Debug.Log("조합마법 공격");

        TryCastSkill();
    }

    //try cast skill on move 만들기
    
    
}
