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
        Gizmos.color = Color.red;
        Gizmos.DrawLine(groundcheck.transform.position, transform.position + Vector3.down * checkdistance);
    }
    public bool IsGrounded()
    {
        return Physics2D.Raycast(groundcheck.transform.position, Vector2.down, checkdistance,groundlayer);
        
         
    }
}
