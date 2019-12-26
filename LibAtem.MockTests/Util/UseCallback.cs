using System;

namespace LibAtem.MockTests.Util
{
    public class UseCallback<T> : IDisposable
    {
        private readonly Action cleanup;

        public UseCallback(T callback, Action<T> add, Action<T> remove)
        {
            add(callback);
            cleanup = () => remove(callback);
        }
        
        public void Dispose()
        {
            cleanup();
        }
    }
}
