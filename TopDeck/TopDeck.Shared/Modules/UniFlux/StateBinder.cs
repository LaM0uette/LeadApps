namespace BFlux;

public class StateBinder
{
    #region Statements

    private readonly Store _store;

    public StateBinder(Store store)
    {
        _store = store;
    }

    #endregion

    #region Methods

    public StateBinder Add<TState>(TState state) where TState : IState
    {
        _store.AddState(state);
        return this;
    }

    public StateBinder Add<TState>(params object[] args) where TState : IState
    {
        object? instance = Activator.CreateInstance(typeof(TState), args);

        if (instance is not TState state)
            throw new InvalidOperationException($"Impossible to create an instance of {typeof(TState)} with the arguments provided");

        _store.AddState(state);
        return this;
    }

    #endregion
}