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
    [SerializeField] CharacterController controller;
    [SerializeField] float moveSpeed;
    public Vector3 moveDir = new();
    bool isMove;

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
    private void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();

        moveDir.x = input.x;
        moveDir.z = input.y;

        if (input.x == 0 && input.y == 0)
            isMove = false;
        else
            isMove = true;
    }

    private void Move()
    {
        controller.Move(transform.right * moveDir.x * moveSpeed * Time.deltaTime);
        controller.Move(transform.forward * moveDir.z * moveSpeed * Time.deltaTime);
    }
    #endregion
}