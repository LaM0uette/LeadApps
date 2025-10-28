namespace BFlux;

public class SignalBinder
{
    #region Statements

    private readonly Store _store;

    public SignalBinder(Store store)
    {
        _store = store;
    }

    #endregion

    #region Methods

    public SignalBinder Add<TSignal>() where TSignal : ISignal
    {
        _store.AddSignal<TSignal>();
        return this;
    }

    #endregion
}