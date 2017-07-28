using System.Text;

namespace NetCoreNetty.Predefined.Codecs.WebSockets
{
    public class WebSocketFrame
    {
        private string _text = null;

        public WebSocketFrameType Type { get; set; }

        public bool IsFinal { get; set; }

        // TODO:
        public byte[] Bytes { get; set; }

        // TODO: временная хрень
        public string Text
        {
            get
            {
                if (_text == null)
                {
                    _text = GetFrameText();
                }
                return _text;
            }
        }

        // TODO: временная хрень
        private string GetFrameText()
        {
            return Encoding.UTF8.GetString(Bytes);
        }
    }
}