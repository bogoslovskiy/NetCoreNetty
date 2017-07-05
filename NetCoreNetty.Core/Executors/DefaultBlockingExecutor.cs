using System;
using System.Threading;

namespace NetCoreNetty.Core.Executors
{
    public class DefaultBlockingExecutor : IExecutor
    {
        // TODO: Счетчики занятости и планирования на основе битовой строки
        
        private readonly object _lock = new object();
        
        public void Run(Action<object> action, object arg)
        {
            lock (_lock)
            {
                // Проверяем свободные потоки (счетчик занятости).
                
                // while [нет свободных потоков] -> Monitor.Wait(_lock)
                
                // Есть свободный поток (который заблокирован в ожидании задания) ->
                
                // Обновляем счетчик занятости
                
                // Обновляем счетчик планирования
            }
            
            // Устанавливаем параметры выполнения (делегат и аргумент(ы))
                
            // Разблокируем поток (сигнализируем потоку).
        }

        private void SchedulerThreadLoop()
        {
            // 
        }
        
        private void ThreadLoop()
        {
            // 
        }
    }
}