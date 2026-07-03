using UnityEngine;

/// <summary>
/// 锚挂住平台后，将角色拉至平台上方，到达后销毁锚链并回到 idle/move。
/// </summary>
public class PlayerAnchorPullState : PlayerState
{
    private readonly Vector2 anchorPosition;
    private readonly Collider2D platformCollider;

    private Vector2 targetPosition;
    private float pullSpeed = 10f;
    private float arrivalThreshold = 0.05f;
    private float originalGravityScale;
    private bool hasArrived;

    public PlayerAnchorPullState(
        PlayerStateMachine stateMachine,
        player player,
        Vector2 anchorPosition,
        Collider2D platformCollider)
        : base(stateMachine, player, "AnchorPull")
    {
        this.anchorPosition = anchorPosition;
        this.platformCollider = platformCollider;
    }

    public override void OnEnter()
    {
        base.OnEnter();

        float halfHeight = player.cap != null ? player.cap.bounds.extents.y : 0.5f;
        targetPosition = new Vector2(
            anchorPosition.x,
            platformCollider.bounds.max.y + halfHeight + player.AnchorLandOffset);

        originalGravityScale = player.rb.gravityScale;
        player.rb.gravityScale = 0f;
        player.rb.velocity = Vector2.zero;
        player.anim?.Play("move");
    }

    public override void OnFixedUpdate()
    {
        if (hasArrived)
            return;

        player.ActiveChain?.UpdateOrigin(player.transform.position);

        Vector2 nextPosition = Vector2.MoveTowards(
            player.transform.position,
            targetPosition,
            pullSpeed * Time.fixedDeltaTime);

        player.rb.MovePosition(nextPosition);

        if (Vector2.Distance(nextPosition, targetPosition) <= arrivalThreshold)
        {
            player.rb.MovePosition(targetPosition);
            player.rb.velocity = Vector2.zero;
            hasArrived = true;
        }
    }

    public override void OnExit()
    {
        player.rb.gravityScale = originalGravityScale;
        player.rb.velocity = Vector2.zero;
    }

    protected override void HandleTransition()
    {
        if (!hasArrived)
            return;

        player.ClearActiveChain();
        ExitToLocomotionState();
    }

    private void ExitToLocomotionState()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        if (Mathf.Abs(moveX) > 0.01f || Mathf.Abs(moveY) > 0.01f)
            stateMachine.ChangeState(new PlayerMoveState(stateMachine, player));
        else
            stateMachine.ChangeState(new PlayerIdleState(stateMachine, player));
    }
}
