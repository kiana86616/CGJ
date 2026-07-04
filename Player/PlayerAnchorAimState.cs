using UnityEngine;

/// <summary>
/// 扔锚-瞄准状态 —— 玩家进入瞄准模式，选择锚的投掷方向。
/// 条件：按瞄准键 → 进入瞄准
///       按发射键 → 切换到发射状态
///       按取消键 → 返回静止/移动状态
/// </summary>
public class PlayerAnchorAimState : PlayerState
{
    // 瞄准相关
    private Vector2 aimDirection;
    private Vector3 mouseWorldPosition;
    private Camera mainCamera;

    // 输入
    private bool launchPressed;
    private bool cancelPressed;

    // 输入缓冲：防止进入瞄准的同一帧就退出
    private readonly float aimBufferTime = 0.15f;
    private float aimBufferTimer;

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

        player.anim?.Play("aim");
        Debug.Log("[PlayerAnchorAimState] 进入瞄准状态");
    }

    public override void OnUpdate()
    {
        aimBufferTimer += Time.deltaTime;

        // 缓冲期内不检测状态切换
        if (aimBufferTimer >= aimBufferTime)
        {
            base.OnUpdate();
        }

        UpdateAimDirection();
    }

    public override void OnExit()
    {
        base.OnExit();
        Debug.Log("[PlayerAnchorAimState] 退出瞄准状态");
    }

    /// <summary>
    /// 根据鼠标位置更新瞄准方向
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

        // 可视：在 Scene 视图中绘制瞄准线
        Debug.DrawRay(player.transform.position, aimDirection * 3f, Color.red);
    }

    protected override void HandleTransition()
    {
        // 松开左键时抛出锚
        launchPressed = Input.GetButtonUp("Fire1");

        // 按 Escape 取消瞄准
        cancelPressed = Input.GetKeyDown(KeyCode.Escape);

        if (launchPressed)
        {
            stateMachine.ChangeState(new PlayerAnchorLaunchState(stateMachine, player, aimDirection));
            return;
        }

        if (cancelPressed)
        {
            // 取消瞄准，返回静止状态
            stateMachine.ChangeState(new PlayerIdleState(stateMachine, player));
            return;
        }
    }

    /// <summary>获取当前瞄准方向（供外部读取）</summary>
    public Vector2 GetAimDirection() => aimDirection;
}
