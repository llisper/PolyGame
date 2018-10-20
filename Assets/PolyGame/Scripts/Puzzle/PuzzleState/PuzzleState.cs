using UnityEngine;
using System;
using System.Collections.Generic;

public abstract class PuzzleState
{
    public PuzzleStateMachine stateMachine;

    protected Puzzle Data { get { return stateMachine.Data; } }
    protected float scrambleRadius { get { return Config.Instance.puzzle.scrambleRadius; } }
    protected float moveSpeed { get { return Config.Instance.puzzle.moveSpeed; } }
    protected float fadeSpeed { get { return Config.Instance.puzzle.fadeSpeed; } }
    protected float finishDebrisMoveSpeed { get { return Config.Instance.puzzle.finishDebrisMoveSpeed; } }

    protected void Next<T>(params object[] p) where T : PuzzleState
    {
        stateMachine.Next<T>();
    }

    public virtual void Start(params object[] p) { }
    public virtual void Update() { }
    public virtual void End() { }

    public virtual bool OnObjPicked(Transform objPicked) { return false; }
    public virtual void OnObjMove(Transform objPicked, Vector2 screenCurrent) { }
    public virtual void OnObjReleased(Transform objPicked) { }
}

public class PuzzleStateMachine
{
    Dictionary<Type, PuzzleState> states = new Dictionary<Type, PuzzleState>();

    public Puzzle Data { get; private set; }
    public PuzzleState Current { get; private set; }

    public PuzzleStateMachine(Puzzle puzzle)
    {
        Data = puzzle;
    }

    public void Add<T>(bool startState = false) where T : PuzzleState, new()
    {
        var state = new T();
        state.stateMachine = this;
        if (startState)
            Current = state;
        states.Add(state.GetType(), state);
    }

    public void Start()
    {
        Current.Start();
    }

    public void Update()
    {
        if (null != Current)
            Current.Update();
    }

    public void Next<T>(params object[] p) where T : PuzzleState
    {
        Next(typeof(T), p);
    }

    public void Next(Type type, params object[] p)
    {
        var next = states[type];
        if (null != Current)
            Current.End();
        Current = next;
        Current.Start(p);
    }
}
