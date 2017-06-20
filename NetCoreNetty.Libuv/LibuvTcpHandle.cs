using System;
using System.Net;

namespace NetCoreNetty.Libuv
{
    public class LibuvTcpHandle : LibuvStreamHandle
    {
        public void Init(LibuvLoopHandle loopHandle)
        {
            int tcpHandleSize = LibuvNative.uv_handle_size(LibuvNative.HandleType.TCP);

            InitUnmanaged(tcpHandleSize);

            LibuvNative.uv_tcp_init(loopHandle, this);
        }

        public void Bind(ServerAddress address)
        {
            IPEndPoint endpoint = CreateIPEndpoint(address);

            var addressText = endpoint.Address.ToString();

            SockAddr addr;
            int ip4Status = LibuvNative.uv_ip4_addr(addressText, endpoint.Port, out addr);

            if (ip4Status < 0)
            {
                int ip6Status = LibuvNative.uv_ip6_addr(addressText, endpoint.Port, out addr);
                if (ip6Status < 0)
                {
                    // TODO:
                    throw new Exception();
                }
            }

            LibuvNative.uv_tcp_bind(this, ref addr, 0 /* flags */);
        }

        static private IPEndPoint CreateIPEndpoint(ServerAddress address)
        {
            // TODO: IPv6 support
            IPAddress ip;

            if (!IPAddress.TryParse(address.Host, out ip))
            {
                if (string.Equals(address.Host, "localhost", StringComparison.OrdinalIgnoreCase))
                {
                    ip = IPAddress.Loopback;
                }
                else
                {
                    ip = IPAddress.IPv6Any;
                }
            }

            return new IPEndPoint(ip, address.Port);
        }
    }
}