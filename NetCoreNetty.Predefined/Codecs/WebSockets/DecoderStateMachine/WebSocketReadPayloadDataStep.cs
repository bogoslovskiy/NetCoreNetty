using System;
using NetCoreNetty.Buffers;

namespace NetCoreNetty.Predefined.Codecs.WebSockets.DecoderStateMachine
{
    class WebSocketReadPayloadDataStep : IWebSocketDecoderStep
    {
        private IWebSocketDecoderStep _readHeaderStep;
        
        private readonly int _frameMaxSize;

        private int _readIndex;
        private int _readToIndex;
        private WebSocketFrame _frame;
        
        public WebSocketReadPayloadDataStep(int frameMaxSize)
        {
            _frameMaxSize = frameMaxSize;
        }

        public void Init(IWebSocketDecoderStep readHeaderStep)
        {
            _readHeaderStep = readHeaderStep;
        }
        
        public void Clear()
        {
            _readIndex = 0;
            _readToIndex = 0;
            _frame = null;
        }

        public void Read(
            ByteBuf byteBuf,
            ref WebSocketReadState state,
            out WebSocketFrame frame,
            out IWebSocketDecoderStep nextStep)
        {
            frame = null;
            nextStep = null;

            bool continueRead = false;
            
            if (_frame == null)
            {
                int payloadLen = (int) state.GetPayloadLen();
                int remainPayloadLen = payloadLen - _readIndex;
                payloadLen = Math.Min(
                    payloadLen, 
                    Math.Min(remainPayloadLen, _frameMaxSize)
                );

                _readToIndex = _readIndex + payloadLen - 1;
                
                continueRead = _readToIndex < (int)state.GetPayloadLen() - 1;
                
                _frame = new WebSocketFrame();
                _frame.IsFinal = !continueRead && state.Fin;
                _frame.Type = Utils.GetFrameType(state.OpCode);
                _frame.Bytes = new byte[payloadLen];
            }
            
            if (byteBuf.ReadableBytes() > 0)
            {
                byte[] frameBytes = _frame.Bytes;

                int i = 0;
                while (_readIndex <= _readToIndex)
                {
                    if (state.Mask)
                    {
                        frameBytes[i] = (byte) (byteBuf.ReadByte() ^ state.MaskingKey[_readIndex % 4]);
                    }
                    else
                    {
                        frameBytes[i] = byteBuf.ReadByte();
                    }

                    i++;
                    _readIndex++;
                }

                if (_readIndex == _readToIndex + 1)
                {
                    frame = _frame;
                    _frame = null;

                    if (!continueRead)
                    {
                        Clear();
                        state.Clear();

                        nextStep = _readHeaderStep;
                    }
                }
            }
        }
    }
}