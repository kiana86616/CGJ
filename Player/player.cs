using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player : MonoBehaviour
{
    private PlayerStateMachine stateMachine;
    // 公开组件引用，供状态类使用
    public Rigidbody2D rb{ get; private set;}
    public CapsuleCollider2D cap{ get; private set;}
    public Animator anim{ get; private set;}
    public GameObject anchor;
    public GameObject aimdot;
    public Transform groundcheck;
    public float checkdistance = 2f;
    public LayerMask groundlayer;

    // 当前脚下地面的信息
    private RaycastHit2D groundHit;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();

        // 初始化状态机，默认进入静止状态
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
    }

    private void OnDrawGizmos()
    {
        if (groundcheck == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(groundcheck.position, transform.position + Vector3.down * checkdistance);
    }
    public bool IsGrounded()
    {
        // Debug.Log("IsGrounded");
        return Physics2D.Raycast(groundcheck.position, Vector2.down, checkdistance,groundlayer);
    }

    /// <summary>当前激活的锁链引用</summary>
    public AnchorChain activeChain;

    /// <summary>锚挂住平台后，开始拉拽玩家</summary>
    public void StartAnchorPull(Vector2 attachPoint, Collider2D platformCollider)
    {
        Debug.Log($"[player] 锚已挂住 {platformCollider.name}，开始拉拽");
        // TODO: 实现拉拽逻辑（向 attachPoint 施加力/移动）
    }

    /// <summary>锁链完全收回后清理引用</summary>
    public void ClearActiveChain()
    {
        if (activeChain != null)
        {
            Destroy(activeChain.gameObject);
            activeChain = null;
        }
    }
}
