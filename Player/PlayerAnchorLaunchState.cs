using System.Collections;
using UnityEngine;

/// <summary>
/// 扔锚-发射状态 —— 向瞄准方向投掷锚。
/// 条件：发射后短暂停留，锚飞行结束 → 返回静止状态
/// </summary>
public class PlayerAnchorLaunchState : PlayerState
{
    // 锚的飞行参数
    [SerializeField] private float launchForce = 12f;
    [SerializeField] private float anchorGravityScale = 1.5f;

    // 锚预制体引用
    private GameObject anchorPrefab;
    private GameObject launchedAnchor;

    // 发射方向
    private Vector2 launchDirection;

    // 发射后的冷却时间
    private float launchDuration = 0.5f;
    private float launchTimer;

    // 是否已完成发射
    private bool hasLaunched;

    public PlayerAnchorLaunchState(PlayerStateMachine stateMachine, player player, Vector2 direction)
        : base(stateMachine, player, "AnchorLaunch")
    {
        this.launchDirection = direction.normalized;
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
    /// 发射锚 —— 从玩家位置向瞄准方向发射一个锚投射物。
    /// 如果未设置 prefab，则动态创建一个测试用的锚对象。
    /// </summary>
    private void LaunchAnchor()
    {
        // 尝试从 Resources 加载锚预制体
        anchorPrefab = Resources.Load<GameObject>("Prefabs/Anchor");

        Vector3 spawnPosition = player.transform.position + (Vector3)(launchDirection * 1f);

        if (anchorPrefab != null)
        {
            launchedAnchor = Object.Instantiate(anchorPrefab, spawnPosition, Quaternion.identity);
        }
        else
        {
            // 回退：动态创建一个临时锚对象用于测试
            launchedAnchor = new GameObject("Anchor_Temp");
            launchedAnchor.transform.position = spawnPosition;
            launchedAnchor.tag = "Anchor";

            // 添加 SpriteRenderer
            SpriteRenderer sr = launchedAnchor.AddComponent<SpriteRenderer>();
            sr.sprite = Resources.Load<Sprite>("Sprites/Anchor");
            sr.color = Color.gray;

            // 添加碰撞体
            CircleCollider2D col = launchedAnchor.AddComponent<CircleCollider2D>();
            col.radius = 0.3f;
        }

        // 添加刚体并施加力
        Rigidbody2D anchorRb = launchedAnchor.GetComponent<Rigidbody2D>();
        if (anchorRb == null)
        {
            anchorRb = launchedAnchor.AddComponent<Rigidbody2D>();
        }

        anchorRb.gravityScale = anchorGravityScale;
        anchorRb.velocity = launchDirection * launchForce;

        // 给锚添加旋转效果
        anchorRb.angularVelocity = 360f * Mathf.Sign(launchDirection.x);

        Debug.Log($"[PlayerAnchorLaunchState] 锚已发射！速度: {launchDirection * launchForce}");

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
