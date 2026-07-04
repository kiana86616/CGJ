using System.Collections;
using UnityEngine;

/// <summary>
/// 扔锚-发射状态 —— 向瞄准方向投掷锚。
/// 条件：发射后短暂停留，锚飞行结束 → 返回静止状态
/// </summary>
public class PlayerAnchorLaunchState : PlayerState
{
    // 锚的飞行参数
    [SerializeField] private float anchorGravityScale = 1.5f;

    private GameObject launchedAnchor;

    // 发射数据
    private Vector2 launchDirection;
    private float actualLaunchForce;

    // 发射后的冷却时间
    private float launchDuration = 0.5f;
    private float launchTimer;

    // 是否已完成发射
    private bool hasLaunched;

    public PlayerAnchorLaunchState(PlayerStateMachine stateMachine, player player, Vector2 direction, float launchForce)
        : base(stateMachine, player, "AnchorLaunch")
    {
        this.launchDirection = direction.normalized;
        this.actualLaunchForce = launchForce;
    }

    public override void OnEnter()
    {
        base.OnEnter();

        hasLaunched = false;
        launchTimer = 0f;

        // 播放发射动画
        player.anim?.Play("launch");

        // 翻转角色朝向发射方向
        if (launchDirection.x != 0)
        {
            player.transform.localScale = new Vector3(
                Mathf.Sign(launchDirection.x) * Mathf.Abs(player.transform.localScale.x),
                player.transform.localScale.y,
                player.transform.localScale.z
            );
        }

        // 执行发射
        LaunchAnchor();

        Debug.Log($"[PlayerAnchorLaunchState] 进入发射状态，方向: {launchDirection}");
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        // 发射后的短暂停留
        if (!hasLaunched) return;

        launchTimer += Time.deltaTime;
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        // 发射期间保持静止
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
    /// 发射锚 —— 从 player.anchor 获取预制体，生成实例并传入发射数据。
    /// </summary>
    private void LaunchAnchor()
    {
        if (player.anchor == null)
        {
            Debug.LogError("[PlayerAnchorLaunchState] player.anchor 未设置！请在 player 上配置锚预制体。");
            hasLaunched = true;
            return;
        }

        Vector3 spawnPosition = player.transform.position + (Vector3)(launchDirection * 0.5f);
        launchedAnchor = Object.Instantiate(player.anchor, spawnPosition, Quaternion.identity);

        // 获取或添加 Rigidbody2D，传入发射数据
        Rigidbody2D anchorRb = launchedAnchor.GetComponent<Rigidbody2D>();
        if (anchorRb == null)
        {
            anchorRb = launchedAnchor.AddComponent<Rigidbody2D>();
        }

        anchorRb.gravityScale = anchorGravityScale;
        anchorRb.velocity = launchDirection * actualLaunchForce;

        // 锚的朝向跟随速度方向
        // float angle = Mathf.Atan2(anchorRb.velocity.y, anchorRb.velocity.x) * Mathf.Rad2Deg;
        // launchedAnchor.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        hasLaunched = true;
    }

    protected override void HandleTransition()
    {
        if (!hasLaunched) return;

        // 发射后等待短暂冷却，然后返回静止或移动状态
        if (launchTimer >= launchDuration)
        {
            // 检查是否有移动输入来决定返回哪个状态
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
