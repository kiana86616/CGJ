using UnityEngine;

/// <summary>
/// 锚链状态：右键进入瞄准，左键发射，MixTime 冷却后返回 idle/move。
/// </summary>
public class PlayerAnchorAimState : PlayerState
{
    private enum Phase { Aiming, Cooldown }

    private Phase phase;
    private Vector2 aimDirection;
    private Camera mainCamera;
    private AnchorChain launchedChain;
    private float cooldownTimer;
    private readonly float aimBufferTime = 0.15f;
    private float aimBufferTimer;

    public PlayerAnchorAimState(PlayerStateMachine stateMachine, player player)
        : base(stateMachine, player, "AnchorAim")
    {
    }

    public override void OnEnter()
    {
        base.OnEnter();
        phase = Phase.Aiming;
        aimDirection = Vector2.right;
        aimBufferTimer = 0f;
        mainCamera = Camera.main;

        player.rb.velocity = Vector2.zero;
        player.anim?.Play("aim");
    }

    public override void OnUpdate()
    {
        if (phase == Phase.Aiming)
        {
            aimBufferTimer += Time.deltaTime;
            UpdateAimDirection();
            if (aimBufferTimer >= aimBufferTime)
                HandleAimingTransition();
        }
        else
        {
            cooldownTimer += Time.deltaTime;
            if (player.ActiveChain != null && player.ActiveChain.HasAttached)
                return;

            if (cooldownTimer >= player.MixTime)
                ExitToLocomotionState();
        }
    }

    public override void OnFixedUpdate()
    {
        if (phase == Phase.Cooldown)
            player.rb.velocity = Vector2.zero;
    }

    private void UpdateAimDirection()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
        if (mainCamera == null)
            return;

        Vector2 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 toMouse = mouseWorld - (Vector2)player.transform.position;
        aimDirection = toMouse.sqrMagnitude > 0.0001f ? toMouse.normalized : Vector2.right;

        Debug.DrawLine(player.transform.position, (Vector2)player.transform.position + aimDirection * player.AnchorChainMaxLength, Color.red);
    }

    private void HandleAimingTransition()
    {
        if (Input.GetButtonDown("Fire2") || Input.GetKeyDown(KeyCode.Escape))
        {
            stateMachine.ChangeState(new PlayerIdleState(stateMachine, player));
            return;
        }

        if (!Input.GetButtonDown("Fire1") || !player.CanLaunchAnchor())
            return;

        FireChain();
    }

    private void FireChain()
    {
        if (aimDirection.x != 0)
        {
            player.transform.localScale = new Vector3(
                Mathf.Sign(aimDirection.x) * Mathf.Abs(player.transform.localScale.x),
                player.transform.localScale.y,
                player.transform.localScale.z);
        }

        player.anim?.Play("launch");
        launchedChain = player.SpawnChain(aimDirection);
        if (launchedChain == null)
            return;

        if (launchedChain.HasAttached)
            return;

        phase = Phase.Cooldown;
        cooldownTimer = 0f;
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

    protected override void HandleTransition() { }
}
