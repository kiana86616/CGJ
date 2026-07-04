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
        : base(stateMachine, player, "move")
    {
    }

    public override void OnEnter()
    {
        base.OnEnter();
        if (player.anim != null)
            player.anim.Play("move");
        Debug.Log("[PlayerMoveState] 进入移动状态");
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        // 获取输入方向
        moveInputX = Input.GetAxisRaw("Horizontal");
        moveDirection = new Vector2(moveInputX, 0f).normalized;

        // 应用水平移动，保持垂直速度不变（让重力/跳跃自然处理）
        player.rb.velocity = new Vector2(moveDirection.x * moveSpeed, player.rb.velocity.y);
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

        // 按住左键进入瞄准
        if (Input.GetButton("Fire1"))
        {
            stateMachine.ChangeState(new PlayerAnchorAimState(stateMachine, player));
            return;
        }
    }
}
