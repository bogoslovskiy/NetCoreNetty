using System;
using System.Threading.Tasks;
using NetCoreNetty.Core;

namespace NetCoreNetty.Concurrency
{
    public class FastInboundBuffer : FastProducerConsumerBuffer<ChannelReadData>, IInboundBuffer
    {
        private readonly int _consumersCount;
        private Task[] _consumerTasks;
        
        public FastInboundBuffer(int size, int consumersCount) 
            : base(size)
        {
            _consumersCount = consumersCount;
        }

        public void StartConsumingTask()
        {
            if (_consumerTasks != null)
            {
                throw new Exception();
            }
            
            _consumerTasks = new Task[_consumersCount];

            for (int i = 0; i < _consumersCount; i++)
            {
                Task consumerTask = Task.Factory.StartNew(
                    obj =>
                    {
                        var buffer = (IInboundBuffer) obj;
                        buffer.StartRead(ProcessChannelData);
                    },
                    this
                );
                _consumerTasks[i] = consumerTask;
            }
        }
        
        static private void ProcessChannelData(ChannelReadData data)
        {
            data.ChannelPipeline.ChannelReadCallback(data.ChannelPipeline.Channel, data.ByteBuffer);
        }
    }
}