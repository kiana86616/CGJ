using UnityEngine;

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerStateMachine stateMachine, player player)
        : base(stateMachine, player, "Idle") { }

    public override void OnEnter()
    {
        base.OnEnter();
        // 只清空水平速度，保留垂直速度（重力下落不受影响）
        Vector2 vel = player.rb.velocity;
        vel.x = 0;
        player.rb.velocity = vel;
        player.anim?.Play("idle");
    }

    protected override void HandleTransition()
    {
        float moveInputX = Input.GetAxisRaw("Horizontal");
        float moveInputY = Input.GetAxisRaw("Vertical");

        if (Mathf.Abs(moveInputX) > 0.01f || Mathf.Abs(moveInputY) > 0.01f)
        {
            stateMachine.ChangeState(new PlayerMoveState(stateMachine, player));
            return;
        }

        if (Input.GetButtonDown("Fire2"))
        {
            stateMachine.ChangeState(new PlayerAnchorAimState(stateMachine, player));
        }
    }
}