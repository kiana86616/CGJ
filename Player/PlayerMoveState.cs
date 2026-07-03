using UnityEngine;

/// <summary>
/// 移动状态 —— 玩家根据输入进行水平移动，保留垂直速度。
/// </summary>
public class PlayerMoveState : PlayerState
{
    private float moveSpeed = 5f; // 可改为从 player 获取

    private float moveInputX;
    private float moveInputY;

    public PlayerMoveState(PlayerStateMachine stateMachine, player player)
        : base(stateMachine, player, "move")
    {
    }

    public override void OnEnter()
    {
        base.OnEnter();
        player.anim?.Play("move");
        Debug.Log("[PlayerMoveState] 进入移动状态");
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        moveInputX = Input.GetAxisRaw("Horizontal");
        // 只修改 X 轴速度，Y 轴保持物理模拟（重力）不受干扰
        Vector2 velocity = player.rb.velocity;
        velocity.x = moveInputX * moveSpeed;
        player.rb.velocity = velocity;
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

        // 判断是否有移动输入
        if (Mathf.Abs(moveInputX) < 0.01f && Mathf.Abs(moveInputY) < 0.01f)
        {
            stateMachine.ChangeState(new PlayerIdleState(stateMachine, player));
            return;
        }

        // 瞄准输入（右键）
        if (Input.GetButtonDown("Fire2"))
        {
            stateMachine.ChangeState(new PlayerAnchorAimState(stateMachine, player));
            return;
        }
    }
}