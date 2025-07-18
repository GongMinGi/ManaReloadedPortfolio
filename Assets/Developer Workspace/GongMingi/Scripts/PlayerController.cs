using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

/// <summary>
/// 개발자: 이예린, 공민기
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
    [SerializeField] private Key castingCompleteKey = Key.Space;            // 캐스팅 확정 키

    [SerializeField]
    ElementalRangedAttackController rangedAttackController; 


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


    #endregion


    #region Unity Update

    /// <summary>
    /// 물리 프레임마다 호출.
    /// - <see cref="Move"/>로 실제 이동을 수행하고  
    /// - 이동 중이면 <see cref="MapTileManager.UpdateCurrentPos"/>를 호출해
    ///   무한 맵 타일 위치를 갱신한다.
    /// </summary>
    private void FixedUpdate()
    {
        Move();
        if (isMove)
            MapTileManager.Instance.UpdateCurrentPos();
    }

    private void Start()
    {
        MapTileManager.Instance.Player = this;

        // 원거리 공격을 위한 초기 세팅 작업
        foreach (var mapping in castingKeyMapping)
            rangedAttackController.CastedElementCount.Add(mapping.Value, 0);
    }
    #endregion

    #region Move


    /// <summary>
    /// 스프린트(Shift) 입력 처리.
    /// - <paramref name="ctx"/>.started → isSprint = true
    /// - <paramref name="ctx"/>.canceled → isSprint = false
    /// </summary>
    public void OnSprint(InputAction.CallbackContext ctx)
    {
        if (ctx.started) isSprint = true;
        if (ctx.canceled) isSprint = false;
        else return;

        //Debug.Log($"val : {value}, {value.Get<float>()}");
        //isSprint = value.Get<float>() >0.5f ? true : false;
    }


    /// <summary>
    /// WASD 이동 입력 처리.
    /// - 입력 벡터를 moveDir에 저장하고 
    /// - 0,0 여부에 따라 isMove 플래그를 갱신한다.
    /// </summary>
    /// <param name="value">InputAction 콜백으로 전달된 Vector2 값</param>
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


    /// <summary>
    /// 실제 CharacterController 이동 로직.
    /// - 좌 Ctrl이 눌려 있으면 이동을 중단한다.  
    /// - 스프린트 중이면 sprintMultiplier 를 speed에 곱해주어 속도를 증가시킨다.
    /// </summary>
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

    /// <summary>
    /// 속성 키(WASD) 입력 처리.
    /// - 키보드 키를 <see cref="E_CastingType"/>로 매핑해 <see cref="AddCasting"/> 호출.  
    /// - 홀드/릴리스 구분 없이 ctx.started 상태에서만 동작.
    /// </summary>
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


    /// <summary>
    /// 캐스팅 리스트에 새 속성을 추가하고 UI 이벤트를 발행한다.
    /// - maxInputCount 초과 시 무시.  
    /// - <see cref="onCastAdded"/> (타입, 인덱스) 이벤트 호출.
    /// </summary>
    private void AddCasting(E_CastingType castingType)
    {
        if (currentCastingList.Count >= maxInputCount) return;          //초과 입력 무시

        currentCastingList.Add(castingType);
        rangedAttackController.CastedElementCount[castingType]++;   // 현재 캐스팅된 속성 개수 업데이트
        onCastAdded.Invoke(castingType, currentCastingList.Count - 1);  // unity event
    }



    /// <summary>
    /// 캐스팅 입력을 초기화하고 UI 리셋 이벤트(<see cref="onCastReset"/>)를 호출한다.
    /// 
    /// => 이부분 추가로 공부필요 어케작동하는지 아직이해못함 ㅠ
    /// 
    /// </summary>
    private void ResetCasting()
    {
        currentCastingList.Clear();
        onCastReset?.Invoke();
    }


    #endregion


    #region 조합 마법 공격

    /// <summary>
    /// ‘조합 마법 공격’ 입력 트리거.
    /// - ctx.started 상태에서 <see cref="TryCastSkill"/>을 호출한다.
    /// </summary>
    public void OnCombinationMagicAttack(InputAction.CallbackContext ctx)
    {
        if (!ctx.started) return;
        Debug.Log("조합마법 공격");

        TryCastSkill();
    }



    /// <summary>
    /// 현재 캐스팅 리스트와 매칭되는 스킬을 찾아 실행한다.
    /// - <see cref="SkillCastingManager.GetSkill"/>로 조회.  
    /// - 성공 시 ExecuteSkill() 호출, 없으면 오류 로그.  
    /// - 처리 후 <see cref="ResetCasting"/> 수행.
    /// </summary>
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
            Debug.LogWarning("해당 조합에 매칭되는 스킬이 없습니다.");
        }

        ResetCasting();

    }
    #endregion


    #region 근접 공격


    /// <summary>
    /// 근접 공격 입력 트리거.
    /// - 캐스팅이 존재하면 <see cref="TryEnchant"/>를 호출해 인첸트 시도.  
    /// - 없으면 일반 근접 공격(후속 구현 필요) 로그 출력.
    /// </summary>
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


    /// <summary>
    /// 근접 무기 인첸트 시도.
    /// * 실제 인첸트 효과는 추후 구현 예정.
    /// </summary>
    private void TryEnchant()
    {
        Debug.Log("인첸트 실행");

        ResetCasting();
    }

    #endregion


    #region 원거리 공격


    /// <summary>
    /// 원거리 공격 입력 트리거.
    /// - 캐스팅 리스트가 비어 있으면 아무 작업도 하지 않는다.  
    /// - 현재는 로그 출력 후 캐스팅을 초기화한다.
    /// </summary>
    public void OnRangedAttack(InputAction.CallbackContext ctx)
    {
        if (!ctx.started) return;


        if (currentCastingList.Count == 0)
            return;

        Debug.Log("원거리 공격");

        rangedAttackController.TryElementalRangedAttack();  // 원거리 공격 시도

        ResetCasting();

    }


    #endregion

}