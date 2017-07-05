using System;
using System.Runtime.CompilerServices;

namespace NetCoreNetty.Core.Executors
{
    /// <summary>
    /// Обычный executor, выполняющий задачи тем же потоком, который их запросил.
    /// </summary>
    internal class DefaultSameThreadExecutor : IExecutor
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Run(Action<object> action, object arg)
        {
            action(arg);
        }
    }
}