namespace BFlux;

public interface ISignalStore
{
    public void Emit<TSignal>(TSignal signal) where TSignal : ISignal;
    public IDisposable Listen<TSignal>(Action<TSignal> action) where TSignal : ISignal;
    public void Mute<TSignal>(Action<TSignal> action) where TSignal : ISignal;
}