using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;
    private PlayerStateMachine stateMachine;

    // 公开组件引用，供状态类使用
    public Rigidbody2D Rb => rb;
    public Animator Anim => anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

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
