using UnityEngine;

/// <summary>
/// 扔锚-瞄准状态 —— 玩家进入瞄准模式，选择锚的投掷方向。
/// 鼠标离玩家越远，抛出力度越大。
/// 条件：按住左键 → 进入瞄准
///       松开左键 → 切换到发射状态（力度由鼠标距离决定）
///       按 Escape → 返回静止/移动状态
/// </summary>
public class PlayerAnchorAimState : PlayerState
{
    // 瞄准相关
    private Vector2 aimDirection;
    private Vector3 mouseWorldPosition;
    private Camera mainCamera;

    // 输入缓冲：防止进入瞄准的同一帧就退出
    private readonly float aimBufferTime = 0.15f;
    private float aimBufferTimer;

    // 当前力度（由鼠标距离决定）
    private float currentForce;

    // 力度范围
    private const float MinLaunchForce = 5f;
    private const float MaxLaunchForce = 18f;

    // 鼠标距离上限（超过此距离力度达到最大）
    private const float MaxAimDistance = 5f;

    private const float AnchorGravityScale = 1.5f;

    // 轨迹预览
    private GameObject[] trajectoryDots;
    private const int TrajectoryDotCount = 12;
    private const float TrajectoryTimeStep = 0.08f;
    

    public PlayerAnchorAimState(PlayerStateMachine stateMachine, player player)
        : base(stateMachine, player, "AnchorAim")
    {
    }

    public override void OnEnter()
    {
        base.OnEnter();

        // 进入瞄准时停止移动
        player.rb.velocity = Vector2.zero;

        // 缓存主摄像机引用
        mainCamera = Camera.main;

        // 初始化瞄准方向（默认朝右）
        aimDirection = Vector2.right;

        // 重置缓冲计时器，防止同一帧按键触发退出
        aimBufferTimer = 0f;

        // 生成轨迹预览点

        player.anim?.Play("aim");
        Debug.Log("[PlayerAnchorAimState] 进入瞄准状态");
    }

    public override void OnUpdate()
    {
        aimBufferTimer += Time.deltaTime;

        UpdateAimDirection();
        UpdateCurrentForce();
        UpdateTrajectory();

        // 检测发射/取消（每帧检测，不受缓冲限制，防止漏掉 GetButtonUp）
        CheckLaunchInput();

        // 缓冲期过后才检测其他状态切换
        if (aimBufferTimer >= aimBufferTime)
        {
            base.OnUpdate();
        }
    }

    public override void OnExit()
    {
        base.OnExit();

        // 清理轨迹预览点
        ClearTrajectoryDots();

        Debug.Log("[PlayerAnchorAimState] 退出瞄准状态");
    }

    /// <summary>
    /// 检测发射和取消输入 —— 不受缓冲限制，防止漏帧。
    /// </summary>
    private void CheckLaunchInput()
    {
        // 松开左键时抛出锚
        if (Input.GetButtonUp("Fire1"))
        {
            stateMachine.ChangeState(new PlayerAnchorLaunchState(stateMachine, player, aimDirection, currentForce));
            return;
        }

        // 按 Escape 取消瞄准
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            stateMachine.ChangeState(new PlayerIdleState(stateMachine, player));
        }
    }

    /// <summary>
    /// 根据鼠标位置更新瞄准方向。
    /// </summary>
    private void UpdateAimDirection()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        // 获取鼠标在世界空间中的位置
        mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0f;

        // 计算玩家朝向鼠标的方向
        Vector2 playerPosition = player.transform.position;
        aimDirection = ((Vector2)mouseWorldPosition - playerPosition).normalized;

        // 可视：在 Scene 视图中绘制瞄准线（长度 = 力度比例）
        float debugLength = currentForce / MaxLaunchForce * 5f;
        Debug.DrawRay(player.transform.position, aimDirection * debugLength, Color.red);
    }

    /// <summary>
    /// 根据鼠标到玩家的距离计算当前力度。
    /// </summary>
    private void UpdateCurrentForce()
    {
        Vector2 playerPosition = player.transform.position;
        float distance = Vector2.Distance(mouseWorldPosition, playerPosition);
        float t = Mathf.Clamp01(distance / MaxAimDistance);
        currentForce = Mathf.Lerp(MinLaunchForce, MaxLaunchForce, t);
    }

    /// <summary>
    /// 生成并更新轨迹预览点 —— 首次调用时生成，之后每帧根据鼠标位置更新抛物线。
    /// </summary>
    private void UpdateTrajectory()
    {
        if (player.aimdot == null) return;

        // 首次生成轨迹点
        if (trajectoryDots == null)
        {
            trajectoryDots = new GameObject[TrajectoryDotCount];
            for (int i = 0; i < TrajectoryDotCount; i++)
            {
                trajectoryDots[i] = Object.Instantiate(player.aimdot, player.transform.position, Quaternion.identity);
            }
        }

        // 根据当前力度计算初速度
        Vector2 initialVelocity = aimDirection * currentForce;

        // 重力加速度（向下为负）
        float gravity = Physics2D.gravity.y * AnchorGravityScale;

        Vector2 startPos = player.transform.position;

        for (int i = 0; i < trajectoryDots.Length; i++)
        {
            if (trajectoryDots[i] == null) continue;

            float simTime = (i + 1) * TrajectoryTimeStep;

            // 抛物线公式: pos = start + v0*t + 0.5*g*t²
            Vector2 predictedPos = startPos
                + initialVelocity * simTime
                + 0.5f * gravity * simTime * simTime * Vector2.up;

            trajectoryDots[i].transform.position = predictedPos;

            // 越远越透明
            SpriteRenderer sr = trajectoryDots[i].GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                float alpha = 1f - (float)i / trajectoryDots.Length;
                Color c = sr.color;
                c.a = alpha;
                sr.color = c;
            }
        }
    }

    /// <summary>
    /// 清理所有轨迹预览点。
    /// </summary>
    private void ClearTrajectoryDots()
    {
        if (trajectoryDots == null) return;

        for (int i = 0; i < trajectoryDots.Length; i++)
        {
            if (trajectoryDots[i] != null)
            {
                Object.Destroy(trajectoryDots[i]);
            }
        }
        trajectoryDots = null;
    }

    protected override void HandleTransition()
    {
        // 发射/取消逻辑已移至 CheckLaunchInput()，此处保留以供后续扩展
    }

    /// <summary>获取当前瞄准方向（供外部读取）</summary>
    public Vector2 GetAimDirection() => aimDirection;
}
