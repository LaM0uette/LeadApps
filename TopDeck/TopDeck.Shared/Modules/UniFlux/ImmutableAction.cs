namespace BFlux;

public abstract record ImmutableAction<TState> : IAction<TState> where TState : IState
{
    public abstract TState Reduce(TState state);
}