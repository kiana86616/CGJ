using UnityEngine;

/// <summary>
/// 玩家状态机 —— 负责状态的初始化、切换和驱动。
/// </summary>
public class PlayerStateMachine
{
    public PlayerState CurrentState { get; private set; }
    public string CurrentStateName => CurrentState?.StateName;

    private player player;

    public PlayerStateMachine(player player)
    {
        this.player = player;
    }

    /// <summary>使用初始状态启动状态机</summary>
    public void Initialize(PlayerState startState)
    {
        CurrentState = startState;
        CurrentState?.OnEnter();
    }

    public void ChangeState(PlayerState newState)
    {
        if (newState == null)
            return;

        CurrentState?.OnExit();
        CurrentState = newState;
        CurrentState.OnEnter();
    }

    /// <summary>每帧调用，驱动当前状态的 Update</summary>
    public void Update()
    {
        CurrentState?.OnUpdate();
    }

    /// <summary>每帧调用，驱动当前状态的 FixedUpdate</summary>
    public void FixedUpdate()
    {
        CurrentState?.OnFixedUpdate();
    }
}
