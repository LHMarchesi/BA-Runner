using System.Collections.Generic;

public class StateMachine<T>
{
    private IState currentState;
    private Dictionary<T, IState> states = new();

    public IState CurrentState => currentState;


    public void SetCurrent(IState state)
    {
        currentState = state;
    }

    public void AddState(IState state, T stateValue)
    {
        states.Add(stateValue, state);
    }

    public void Update() => currentState.Execute();

    public void ChangeState(T newState)
    {
        if (states.TryGetValue(newState, out IState stateValue))
        {
            currentState?.Sleep();
            currentState = stateValue;
            currentState.Awake();
        }
    }
}