using UnityEngine;

/// <summary>
/// 扔锚-瞄准状态 —— 玩家进入瞄准模式，选择锚的投掷方向。
/// 鼠标离玩家越远，锁链最大长度越大。
/// 条件：按住左键 → 进入瞄准
///       松开左键 → 切换到发射状态（链长由鼠标距离决定）
///       按 Escape → 返回静止/移动状态
/// </summary>
public class PlayerAnchorAimState : PlayerState
{
    // 瞄准相关
    private Vector2 aimDirection;
    private Vector3 mouseWorldPosition;
    private Camera mainCamera;

    // 输入缓冲：防止进入瞄准的同一帧就退出
    private const float AimBufferTime = 0.15f;
    private float aimBufferTimer;

    // 当前链长（由鼠标距离决定）
    private float currentChainLength;

    // 链长范围
    private const float MinChainLength = 5f;
    private const float MaxChainLength = 18f;

    // 鼠标距离上限（超过此距离链长达到最大）
    private const float MaxAimDistance = 8f;

    // 直线预览
    private GameObject[] previewDots;
    private const int PreviewDotCount = 15;

    public PlayerAnchorAimState(PlayerStateMachine stateMachine, player player)
        : base(stateMachine, player, "AnchorAim")
    {
    }

    public override void OnEnter()
    {
        base.OnEnter();

        player.rb.velocity = Vector2.zero;
        mainCamera = Camera.main;
        aimDirection = Vector2.right;
        aimBufferTimer = 0f;

        if (player.anim != null)
            player.anim.Play("aim");
        Debug.Log("[PlayerAnchorAimState] 进入瞄准状态");
    }

    public override void OnUpdate()
    {
        aimBufferTimer += Time.deltaTime;

        UpdateAimDirection();
        UpdateCurrentChainLength();
        UpdatePreviewDots();

        CheckLaunchInput();

        if (aimBufferTimer >= AimBufferTime)
        {
            base.OnUpdate();
        }
    }

    public override void OnExit()
    {
        base.OnExit();
        ClearPreviewDots();
        Debug.Log("[PlayerAnchorAimState] 退出瞄准状态");
    }

    /// <summary>
    /// 检测发射和取消输入 —— 不受缓冲限制，防止漏帧。
    /// </summary>
    private void CheckLaunchInput()
    {
        if (Input.GetButtonUp("Fire1"))
        {
            stateMachine.ChangeState(new PlayerAnchorLaunchState(stateMachine, player, aimDirection, currentChainLength));
            return;
        }

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

        mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0f;

        Vector2 playerPosition = player.transform.position;
        Vector2 raw = (Vector2)mouseWorldPosition - playerPosition;
        if (raw.sqrMagnitude > 0.0001f)
            aimDirection = raw.normalized;

        // Scene 视图调试线
        Debug.DrawRay(player.transform.position, aimDirection * currentChainLength, Color.red);
    }

    /// <summary>
    /// 根据鼠标到玩家的距离计算当前链长。
    /// </summary>
    private void UpdateCurrentChainLength()
    {
        Vector2 playerPosition = player.transform.position;
        float distance = Vector2.Distance(mouseWorldPosition, playerPosition);
        float t = Mathf.Clamp01(distance / MaxAimDistance);
        currentChainLength = Mathf.Lerp(MinChainLength, MaxChainLength, t);
    }

    /// <summary>
    /// 沿瞄准方向绘制直线预览点（匹配 AnchorChain 射线检测逻辑）。
    /// </summary>
    private void UpdatePreviewDots()
    {
        if (player.aimdot == null) return;

        if (previewDots == null)
        {
            previewDots = new GameObject[PreviewDotCount];
            for (int i = 0; i < PreviewDotCount; i++)
            {
                previewDots[i] = Object.Instantiate(player.aimdot, player.transform.position, Quaternion.identity);
            }
        }

        Vector2 startPos = player.transform.position;
        float step = currentChainLength / PreviewDotCount;

        for (int i = 0; i < previewDots.Length; i++)
        {
            if (previewDots[i] == null) continue;

            Vector2 dotPos = startPos + aimDirection * (step * (i + 1));
            previewDots[i].transform.position = dotPos;

            SpriteRenderer sr = previewDots[i].GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                float alpha = 1f - (float)i / previewDots.Length;
                Color c = sr.color;
                c.a = alpha;
                sr.color = c;
            }
        }
    }

    /// <summary>
    /// 清理所有预览点。
    /// </summary>
    private void ClearPreviewDots()
    {
        if (previewDots == null) return;

        for (int i = 0; i < previewDots.Length; i++)
        {
            if (previewDots[i] != null)
                Object.Destroy(previewDots[i]);
        }
        previewDots = null;
    }

    protected override void HandleTransition()
    {
        // 发射/取消逻辑已移至 CheckLaunchInput()
    }

    /// <summary>获取当前瞄准方向（供外部读取）</summary>
    public Vector2 GetAimDirection() => aimDirection;
}
