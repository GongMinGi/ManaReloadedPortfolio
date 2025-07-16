using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 개발자: 이예린
/// 
/// 플레이어 조작을 관리하는 클래스
/// 
/// 현재는 무한 맵 구현을 위해 캐릭터 이동 관련 기능이 구현되어 있음
/// </summary>
public class PlayerController : MonoBehaviour
{


    #region FieldAndProperty

    [SerializeField] CharacterController controller;
    [SerializeField] float moveSpeed;
    [SerializeField] float sprintMultiplier = 2f;


    bool isMove;
    [SerializeField]
    bool isSprint;

    Vector3 moveDir = new();

    /// <summary>
    /// 외부 몬스터에서 접근하기 위해서 moveDir 프로퍼티화
    /// </summary>
    public Vector3 MoveDir
    {
        get => moveDir; 
        set => moveDir = value;
    }

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
        float speed = moveSpeed * (isSprint ? sprintMultiplier : 1f);

        controller.Move(transform.right * moveDir.x * speed * Time.deltaTime);
        controller.Move(transform.forward * moveDir.z * speed * Time.deltaTime);
    }
    #endregion
}