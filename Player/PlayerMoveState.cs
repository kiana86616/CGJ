using UnityEngine;

/// <summary>
/// 移动状态 —— 玩家根据输入进行移动。
/// 条件：有移动输入 → 持续移动
///       无移动输入 → 切换到静止状态
/// </summary>
public class PlayerMoveState : PlayerState
{
    [SerializeField] private float moveSpeed = 5f;

    private float moveInputX;
    private float moveInputY;
    private Vector2 moveDirection;

    public PlayerMoveState(PlayerStateMachine stateMachine, player player)
        : base(stateMachine, player, "Move")
    {
    }

    public override void OnEnter()
    {
        base.OnEnter();
        anim?.Play("Move");
        Debug.Log("[PlayerMoveState] 进入移动状态");
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        // 获取输入方向
        moveInputX = Input.GetAxisRaw("Horizontal");
        moveInputY = Input.GetAxisRaw("Vertical");
        moveDirection = new Vector2(moveInputX, moveInputY).normalized;

        // 应用移动
        rb.velocity = moveDirection * moveSpeed;
    }

    public override void OnExit()
    {
        base.OnExit();
        Debug.Log("[PlayerMoveState] 退出移动状态");
    }

    protected override void HandleTransition()
    {
        moveInputX = Input.GetAxisRaw("Horizontal");
        moveInputY = Input.GetAxisRaw("Vertical");

        if (Mathf.Abs(moveInputX) < 0.01f && Mathf.Abs(moveInputY) < 0.01f)
        {
            stateMachine.ChangeState(new PlayerIdleState(stateMachine, player));
            return;
        }

        // 检测瞄准输入（右键进入瞄准）
        if (Input.GetButtonDown("Fire2"))
        {
            stateMachine.ChangeState(new PlayerAnchorAimState(stateMachine, player));
            return;
        }
    }
}
