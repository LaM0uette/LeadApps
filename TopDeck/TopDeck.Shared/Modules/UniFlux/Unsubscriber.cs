namespace BFlux
{
    internal class Unsubscriber : IDisposable
    {
        #region Statements

        private readonly Action _dispose;

        internal Unsubscriber(Action dispose)
        {
            _dispose = dispose;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _dispose();
        }

        #endregion
    }
}