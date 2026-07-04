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
}
