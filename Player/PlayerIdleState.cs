using UnityEngine;

/// <summary>
/// 静止状态 —— 玩家站在原地，没有移动输入。
/// 条件：无移动输入 → 保持静止
///       有移动输入 → 切换到移动状态
/// </summary>
public class PlayerIdleState : PlayerState
{
    private float moveInputX;
    private float moveInputY;

    public PlayerIdleState(PlayerStateMachine stateMachine, player player)
        : base(stateMachine, player, "Idle")
    {
    }

    public override void OnEnter()
    {
        base.OnEnter();
        // 进入静止状态时停止速度
        player.rb.velocity = Vector2.zero;
        player.anim?.Play("idle");
        Debug.Log("[PlayerIdleState] 进入静止状态");
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
    }

    public override void OnExit()
    {
        base.OnExit();
        Debug.Log("[PlayerIdleState] 退出静止状态");
    }

    protected override void HandleTransition()
    {
        // 检测移动输入
        moveInputX = Input.GetAxisRaw("Horizontal");
        moveInputY = Input.GetAxisRaw("Vertical");

        if (Mathf.Abs(moveInputX) > 0.01f || Mathf.Abs(moveInputY) > 0.01f)
        {
            stateMachine.ChangeState(new PlayerMoveState(stateMachine, player));
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
