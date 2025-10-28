namespace BFlux;

public interface IAction<TState> where TState : IState
{
    public TState Reduce(TState state);
}