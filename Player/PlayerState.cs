using UnityEngine;

/// <summary>
/// 玩家状态基类 —— 所有具体状态都继承自此类。
/// </summary>
public abstract class PlayerState
{
    protected PlayerStateMachine stateMachine;
    protected player player;
    protected Rigidbody2D rb;
    protected Animator anim;

    /// <summary>状态名称，方便调试</summary>
    public string StateName { get; protected set; }

    public PlayerState(PlayerStateMachine stateMachine, player player, string stateName)
    {
        this.stateMachine = stateMachine;
        this.player = player;
        this.rb = player.Rb;
        this.anim = player.Anim;
        this.StateName = stateName;
    }

    /// <summary>进入状态时调用一次</summary>
    public virtual void OnEnter() { }

    /// <summary>每帧 Update 调用</summary>
    public virtual void OnUpdate()
    {
        HandleTransition();
    }

    /// <summary>每帧 FixedUpdate 调用（物理相关逻辑放这里）</summary>
    public virtual void OnFixedUpdate() { }

    /// <summary>退出状态时调用一次</summary>
    public virtual void OnExit() { }

    /// <summary>检测并执行状态切换，子类必须重写</summary>
    protected abstract void HandleTransition();
}
