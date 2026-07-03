using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player : MonoBehaviour
{
<<<<<<< Updated upstream
    private PlayerStateMachine stateMachine;
    // 公开组件引用，供状态类使用
    public Rigidbody2D rb{ get; private set;}
    public Animator anim{ get; private set;}

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
=======
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
>>>>>>> Stashed changes
    }
}
