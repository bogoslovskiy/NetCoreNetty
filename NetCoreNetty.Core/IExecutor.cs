using System;

namespace NetCoreNetty.Core
{
    public interface IExecutor
    {
        void Run(Action<object> action, object arg);
    }
}