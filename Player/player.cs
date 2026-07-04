using UnityEngine;

public class player : MonoBehaviour
{
    private PlayerStateMachine stateMachine;

    // 公开组件引用，供状态类使用
    public Rigidbody2D rb { get; private set; }
    public CapsuleCollider2D cap { get; private set; }
    public Animator anim { get; private set; }
    public GameObject anchor;
    public GameObject aimdot;
    public Transform groundcheck;
    public float checkdistance = 2f;
    public LayerMask groundlayer;

    // 锚拉拽相关
    public AnchorChain activeChain;
    private bool isBeingPulled;
    private Vector2 pullTarget;
    private float pullForce = 15f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        cap = GetComponent<CapsuleCollider2D>();

        stateMachine = new PlayerStateMachine(this);
        stateMachine.Initialize(new PlayerIdleState(stateMachine, this));
    }

    void Update()
    {
        stateMachine?.Update();
    }

    void FixedUpdate()
    {
        stateMachine?.FixedUpdate();

        // 锚拉拽：每帧向挂点施加力
        if (isBeingPulled && activeChain != null && activeChain.HasAttached)
        {
            Vector2 toTarget = pullTarget - (Vector2)transform.position;
            rb.AddForce(toTarget.normalized * pullForce, ForceMode2D.Force);

            // 到达挂点附近则停止拉拽
            if (toTarget.sqrMagnitude < 0.1f)
            {
                StopPull();
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (groundcheck == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(groundcheck.position, transform.position + Vector3.down * checkdistance);
    }

    public bool IsGrounded()
    {
        return Physics2D.Raycast(groundcheck.position, Vector2.down, checkdistance, groundlayer);
    }

    /// <summary>锚挂住平台后，开始拉拽玩家</summary>
    public void StartAnchorPull(Vector2 attachPoint, Collider2D platformCollider)
    {
        pullTarget = attachPoint;
        isBeingPulled = true;
        rb.gravityScale = 0f; // 拉拽时关闭重力
        Debug.Log($"[player] 锚已挂住 {platformCollider.name}，开始拉拽至 {attachPoint}");
    }

    /// <summary>停止拉拽（玩家到达或手动中断）</summary>
    public void StopPull()
    {
        isBeingPulled = false;
        rb.gravityScale = 1f;
        rb.velocity = Vector2.zero;
    }

    /// <summary>锁链完全收回后清理引用</summary>
    public void ClearActiveChain()
    {
        StopPull();

        if (activeChain != null)
        {
            Destroy(activeChain.gameObject);
            activeChain = null;
        }
    }
}
