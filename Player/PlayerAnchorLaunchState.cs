using UnityEngine;

/// <summary>
/// 扔锚-发射状态 —— 用 AnchorChain 系统向瞄准方向伸出锁链。
/// 条件：发射后立即返回静止/移动状态，锁链独立运作。
/// </summary>
public class PlayerAnchorLaunchState : PlayerState
{
    // 发射数据
    private Vector2 launchDirection;
    private float maxChainLength;

    // 发射后的微小缓冲（防止同帧再次触发瞄准）
    private const float LaunchBuffer = 0.1f;
    private float launchTimer;
    private bool hasLaunched;

    public PlayerAnchorLaunchState(PlayerStateMachine stateMachine, player player, Vector2 direction, float launchForce)
        : base(stateMachine, player, "AnchorLaunch")
    {
        launchDirection = direction.normalized;
        // 将力度映射为锁链最大长度（力度 5~18 → 长度 5~18）
        maxChainLength = launchForce;
    }

    public override void OnEnter()
    {
        base.OnEnter();

        hasLaunched = false;
        launchTimer = 0f;

        if (player.anim != null)
            player.anim.Play("launch");

        // 翻转角色朝向发射方向
        if (launchDirection.x != 0)
        {
            player.transform.localScale = new Vector3(
                Mathf.Sign(launchDirection.x) * Mathf.Abs(player.transform.localScale.x),
                player.transform.localScale.y,
                player.transform.localScale.z
            );
        }

        LaunchChain();

        Debug.Log($"[PlayerAnchorLaunchState] 进入发射状态，方向: {launchDirection}，链长: {maxChainLength}");
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (!hasLaunched) return;
        launchTimer += Time.deltaTime;
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        if (!hasLaunched)
        {
            player.rb.velocity = Vector2.zero;
        }
    }

    public override void OnExit()
    {
        base.OnExit();
        Debug.Log("[PlayerAnchorLaunchState] 退出发射状态");
    }

    /// <summary>
    /// 用 AnchorChain 系统发射锁链 —— 实例化锚预制体，调用 Launch。
    /// </summary>
    private void LaunchChain()
    {
        if (player.anchor == null)
        {
            Debug.LogError("[PlayerAnchorLaunchState] player.anchor 未设置！请在 player 上配置锚预制体。");
            hasLaunched = true;
            return;
        }

        // 清理旧链
        player.ClearActiveChain();

        // 实例化锚预制体（应挂有 AnchorChain 组件）
        Vector3 spawnPos = player.transform.position;
        GameObject anchorObj = Object.Instantiate(player.anchor, spawnPos, Quaternion.identity);

        AnchorChain chain = anchorObj.GetComponent<AnchorChain>();
        if (chain == null)
        {
            Debug.LogError("[PlayerAnchorLaunchState] 锚预制体缺少 AnchorChain 组件！");
            Object.Destroy(anchorObj);
            hasLaunched = true;
            return;
        }

        // 用 AnchorChain 接管后续逻辑
        player.activeChain = chain;
        chain.Launch(spawnPos, launchDirection, player, maxChainLength);

        hasLaunched = true;
    }

    protected override void HandleTransition()
    {
        if (!hasLaunched) return;

        // 发射后短暂缓冲，然后返回静止或移动
        if (launchTimer >= LaunchBuffer)
        {
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveY = Input.GetAxisRaw("Vertical");

            if (Mathf.Abs(moveX) > 0.01f || Mathf.Abs(moveY) > 0.01f)
            {
                stateMachine.ChangeState(new PlayerMoveState(stateMachine, player));
            }
            else
            {
                stateMachine.ChangeState(new PlayerIdleState(stateMachine, player));
            }
        }
    }
}
