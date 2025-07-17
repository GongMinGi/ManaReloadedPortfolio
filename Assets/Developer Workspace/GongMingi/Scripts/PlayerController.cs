using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

/// <summary>
/// 개발자: 이예린
/// 
/// 플레이어 조작을 관리하는 클래스
/// 
/// 현재는 무한 맵 구현을 위해 캐릭터 이동 관련 기능이 구현되어 있음
/// </summary>
public class PlayerController : MonoBehaviour
{
    Keyboard keyboard = Keyboard.current;               // 현재 키보드에 대한 제어를 들고 있음?

    #region FieldAndProperty

    [SerializeField] CharacterController controller;
    [SerializeField] float moveSpeed;
    [SerializeField] float sprintMultiplier = 2f;


    [SerializeField] bool isSprint;
    bool isMove;
    //bool canMove;

    Vector3 moveDir = new();

    /// <summary>
    /// 외부 몬스터에서 접근하기 위해서 moveDir 프로퍼티화
    /// </summary>
    public Vector3 MoveDir
    {
        get => moveDir; 
        set => moveDir = value;
    }


    [Header("Casting Settings")]
    [SerializeField] private int maxInputCount = 6;                         // 조합 길이
    [SerializeField] private Key castingCompleteKey = Key.Space;    // 캐스팅 확정 키


    [Header("Event -> UI 연결")]
    public UnityEvent<E_CastingType, int> onCastAdded;                      // (타입, index)
    public UnityEvent onCastReset;

    private readonly List<E_CastingType> currentCastingList = new();


    //[System.]


    private static readonly Dictionary<Key, E_CastingType> castingKeyMapping = new()
    {
        {Key.W, E_CastingType.Fire },
        {Key.A, E_CastingType.Frost },
        {Key.S, E_CastingType.Lightning },
        {Key.D, E_CastingType.Earth },
    };


    #endregion


    #region Unity Update
    private void FixedUpdate()
    {
        Move();
        if (isMove)
            MapTileManager.Instance.UpdateCurrentPos();
    }

    private void Start()
    {
        MapTileManager.Instance.Player = this;
    }
    #endregion

    #region Move

    public void OnSprint(InputAction.CallbackContext ctx)
    {
        if (ctx.started) isSprint = true;
        if (ctx.canceled) isSprint = false;
        else return;

        //Debug.Log($"val : {value}, {value.Get<float>()}");
        //isSprint = value.Get<float>() >0.5f ? true : false;
    }

    //InputValue
    public void OnMove(InputAction.CallbackContext value)
    {

        //if (keyboard.leftCtrlKey.isPressed)
        //{
        //    Debug.Log("컨트롤 눌림");
        //    return;

        //}
        //Vector2 input = value.Get<Vector2>();

        Vector2 input = value.ReadValue<Vector2>();

        moveDir.x = input.x;
        moveDir.z = input.y;

        if (input.x == 0 && input.y == 0)
            isMove = false;
        else
            isMove = true;
    }

    private void Move()
    {
        if (keyboard.leftCtrlKey.isPressed)
        {
            Debug.Log("컨트롤 눌림");
            return;

        }

        float speed = moveSpeed * (isSprint ? sprintMultiplier : 1f);

        //if (keyboard.leftCtrlKey.isPressed)
        //    speed = 0;

        controller.Move(transform.right * moveDir.x * speed * Time.deltaTime);
        controller.Move(transform.forward * moveDir.z * speed * Time.deltaTime);
    }
    #endregion


    #region 속성 캐스팅
    public void OnCastingSpell(InputAction.CallbackContext ctx)
    {
        if (!ctx.started) return;                       // 버튼을 눌렀을 때만 .. 홀드.. 땔때는 모두 리턴

        var keyControl = ctx.control as KeyControl;
        if (keyControl == null) return;                 // 게임패드, 마우스 등 키보드가 아닐 경우 리턴

        Key key = keyControl.keyCode;                   // 새 인풋 시스템의 키 열거형
        Debug.Log(key);

        if (castingKeyMapping.TryGetValue(key, out var castingType))
        {
            AddCasting(castingType);
        }
    }

    private void AddCasting(E_CastingType castingType)
    {
        if (currentCastingList.Count >= maxInputCount) return;      //초과 입력 무시

        currentCastingList.Add(castingType);
        onCastAdded.Invoke(castingType, currentCastingList.Count - 1);  // unity event
    }



    private void ResetCasting()
    {
        currentCastingList.Clear();
        onCastReset?.Invoke();
    }


    #endregion


    #region 조합 마법 공격
    public void OnCombinationMagicAttack(InputAction.CallbackContext ctx)
    {
        if (!ctx.started) return;
        Debug.Log("조합마법 공격");

        TryCastSkill();
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
    #endregion


    #region 근접 공격

    public void OnMeleeAttack(InputAction.CallbackContext ctx)
    {
        if(!ctx.started) return;

        if (currentCastingList.Count != 0)
        {
            TryEnchant();
            return;
        }

        Debug.Log("근접 공격 ");
    }

    private void TryEnchant()
    {
        Debug.Log("인첸트 실행");

        ResetCasting();
    }

    #endregion


    #region 원거리 공격

    public void OnRangedAttack(InputAction.CallbackContext ctx)
    {
        if (!ctx.started) return;


        if (currentCastingList.Count == 0)
            return;

        Debug.Log("원거리 공격");

        ResetCasting();

    }


    #endregion

}