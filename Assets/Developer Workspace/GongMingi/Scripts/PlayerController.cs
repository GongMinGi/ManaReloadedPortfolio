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

    //[SerializeField] CharacterController controller;
    [SerializeField] float moveSpeed;
    [SerializeField] float sprintMultiplier = 2f;
    [SerializeField] Rigidbody rb;

    [SerializeField] bool isSprint;
    [SerializeField] bool isMove;


    [Header("Casting Settings")]
    [SerializeField] private int maxInputCount = 6;                         // 조합 길이

    [SerializeField] ElementalRangedAttackController rangedAttackController;    // 원거리 공격을 제어하는 컨트롤러
    [SerializeField] MeleeConeAttack meleeConeAttack;                           // 근접공격을 실행하기 위한 변수

    [Header("Event -> UI 연결")]
    public UnityEvent<E_CastingType, int> onCastAdded;                      // (타입, index)
    public UnityEvent onCastReset;


    [SerializeField] Animator playerAnim;                                   // 플레이어 애니메이션

    Vector3 moveDir = new();

    /// <summary>
    /// 외부 몬스터에서 접근하기 위해서 moveDir 프로퍼티화
    /// </summary>
    public Vector3 MoveDir
    {
        get => moveDir; 
        set => moveDir = value;
    }

    private readonly List<E_CastingType> currentCastingList = new();

    private static readonly Dictionary<Key, E_CastingType> castingKeyMapping = new()
    {
        {Key.W, E_CastingType.Fire },
        {Key.A, E_CastingType.Light },
        {Key.S, E_CastingType.Thunder },
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
            GameModeManager.MapTileManager.UpdateCurrentPos();
    }

    private void Start()
    {
        Debug.Log("start 진입");
        GameModeManager.Player = this;                                  // 현재 플레이어 인스턴스를 GameModeManager에 등록
        Debug.Log("player 할당");

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
        if (ctx.started) isSprint = true;           // shift를 누르기 시작했을때 달리기 상태로 들어간다.
        if (ctx.canceled) isSprint = false;         // shift에서 손을 땔 때 걷기 상태로 돌아간다.
        else return;
    }


    /// <summary>
    /// WASD 이동 입력 처리.
    /// - 입력 벡터를 moveDir에 저장하고 
    /// - 0,0 여부에 따라 isMove 플래그를 갱신한다.
    /// </summary>
    /// <param name="value">InputAction 콜백으로 전달된 Vector2 값</param>
    public void OnMove(InputAction.CallbackContext value)
    {
        Vector2 input = value.ReadValue<Vector2>();    // wasd로 이동값을 입력받음 

        moveDir.x = input.x;                    // x축이 입력받고 잇는지 , 오른쪽 == 1, 왼쪽 == -1, 정지 == 0
        moveDir.z = input.y;                    // y축이 입력받고 잇는지 , 위쪽 == 1, 아래쪽 == -1, 정지 == 0

        if (input.x == 0 && input.y == 0)       // x축 y축 모두 움직이지 않는다면, 움직임을 판단하는 변수를 false로 설정
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
        if (keyboard.leftCtrlKey.isPressed)                                  // 왼쪽 컨트롤 키가 눌린 상태면 바로 이동 불가
        {
            Debug.Log("컨트롤 눌림");

            rb.linearVelocity = new Vector3(0, 0, 0);                       // 플레이어 즉시 정지

            playerAnim.SetFloat("Horizontal", 0);                           // 플레이어 이동 애니메이션 정지
            playerAnim.SetFloat("Speed", 0);
            return;                                                         

        }

        float speed = moveSpeed * (isSprint ? sprintMultiplier : 1f);       // 이동속도 변수에 달리는 중이면, 다른 숫자를 곱해주고, 아니면  1을 곱해준다


        playerAnim.SetFloat("Horizontal", moveDir.x);
        playerAnim.SetFloat("Speed", moveDir.z);
        

        Vector3 targetVelocity = moveDir.normalized * speed;
        rb.linearVelocity = new Vector3(targetVelocity.x,0, targetVelocity.z);

        //controller.Move(transform.right * moveDir.x * speed * Time.deltaTime);      // 좌우 방향 플레이어 이동 
        //controller.Move(transform.forward * moveDir.z * speed * Time.deltaTime);    // 상하 방향 플레이어 이동
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

        currentCastingList.Add(castingType);                            // 현재 캐스팅된 원소 목록에 지금 누른 원소를 추가한다.
        rangedAttackController.CastedElementCount[castingType]++;       // 현재 캐스팅된 속성 개수 업데이트
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
        currentCastingList.Clear();     // 현재 캐스팅 된 원소들을 지운다
        onCastReset?.Invoke();          // ui에 표시된 원소를 전부 검정색으로 바꾼다. (비운다)
    }

    #endregion


    #region 조합 마법 공격

    /// <summary>
    /// ‘조합 마법 공격’ 입력 트리거.
    /// - ctx.started 상태에서 <see cref="TryCastSkill"/>을 호출한다.
    /// </summary>
    public void OnCombinationMagicAttack(InputAction.CallbackContext ctx)
    {
        if (!ctx.started) return;       // space 를 눌렀을때가 아니면 (hold시 혹은 땠을때) 실행하지 않는다.
        Debug.Log("조합마법 공격");

        TryCastSkill();                 // 스킬 실행
    }



    /// <summary>
    /// 현재 캐스팅 리스트와 매칭되는 스킬을 찾아 실행한다.
    /// - <see cref="SkillCastingManager.GetSkill"/>로 조회.  
    /// - 성공 시 ExecuteSkill() 호출, 없으면 오류 로그.  
    /// - 처리 후 <see cref="ResetCasting"/> 수행.
    /// </summary>
    private void TryCastSkill()
    {
        if (currentCastingList.Count == 0) return;      // 현재 캐스팅된 원소가 없으면 스킬을 실행하지 않는다.

        BaseSkill skill = GameModeManager.SkillCastingManager.GetSkill(currentCastingList);    // 스킬관리자에게 캐스팅된 원소리스트를 보내서 그에 해당하는 스킬의 고유번호를 받는다.

        if (skill != null)              // 스킬이 존재한다면
        {
            skill.ExecuteSkill();       // 스킬을 실행한다.
        }
        else
        {
            Debug.LogWarning("해당 조합에 매칭되는 스킬이 없습니다.");
        }

        ResetCasting();                 // 캐스팅한 속성을 전부 비운다.

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

        meleeConeAttack.ExecuteAttack(E_CastingType.None);      // 무속성 물리 공격 실행

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
        if (!ctx.started) return;       // 마우스 좌클릭을 눌렀을때가 아니면 (hold시 혹은 땠을때) 실행하지 않는다.            


        if (currentCastingList.Count == 0)  // 현재 캐스팅된 원소의 수가 없다면, 리턴한다
            return;

        Debug.Log("원거리 공격");

        rangedAttackController.TryElementalRangedAttack();  // 원거리 공격 시도

        ResetCasting();                     // 캐스팅한 속성을 전부 비운다.

    }


    #endregion

}