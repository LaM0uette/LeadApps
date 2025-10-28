namespace BFlux;

public interface IStateStore
{
    public TState GetState<TState>() where TState : IState;
    public void Dispatch<TState>(IAction<TState> action) where TState : IState;
    public IDisposable Subscribe<TState>(Action<TState> action) where TState : IState;
    public void Unsubscribe<TState>(Action<TState> action) where TState : IState;
}