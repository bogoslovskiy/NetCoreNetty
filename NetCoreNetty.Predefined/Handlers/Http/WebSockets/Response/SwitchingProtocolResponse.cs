using System;
using System.Security.Cryptography;
using NetCoreNetty.Buffers;
using NetCoreNetty.Core;

namespace NetCoreNetty.Handlers.Http.WebSockets.Response
{
    static public class SwitchingProtocolResponse
    {
        [ThreadStatic]
        static private SHA1 _sha1;

        [ThreadStatic]
        static private byte[] _serverAccept;

        [ThreadStatic]
        static private byte[][] _acceptSeeds;

        // TODO: передавать ByteBufProvider
        static public ByteBuf Get(IChannelHandlerContext ctx, byte[] clientAccept, int clientAcceptLength)
        {
            if (_sha1 == null)
            {
                _sha1 = SHA1.Create();
            }

            if (_serverAccept == null)
            {
                // Размер результата SHA1 всегда 160 бит или 20 байт.
                _serverAccept = new byte[20];
            }

            if (_acceptSeeds == null)
            {
                // Длины от 1 до 100.
                _acceptSeeds = new byte[101][];
                for (int i = 1; i <= 100; i++)
                {
                    _acceptSeeds[i] = new byte[i];
                }
            }

            int acceptSeedLength = clientAcceptLength + HttpHeaderConstants.WebSockets13TokenLength;

            byte[] acceptSeed = _acceptSeeds[acceptSeedLength];

            Array.Copy(
                clientAccept /* source array */,
                0 /* sourceIndex */,
                acceptSeed /* destinationArray */,
                0 /* destinationIndex */,
                clientAcceptLength /* length */
            );

            Array.Copy(
                HttpHeaderConstants.WebSockets13Token /* source array */,
                0 /* sourceIndex */,
                acceptSeed /* destinationArray */,
                clientAcceptLength /* destinationIndex */,
                HttpHeaderConstants.WebSockets13TokenLength /* length */
            );

            // TODO: писать прямо _serverAccept без аллокации
            byte[] hashedAccept = _sha1.ComputeHash(acceptSeed);

            // TODO: ToBase64CharArray
            String accept = Convert.ToBase64String(hashedAccept);

            int partLength = HttpHeaderConstants.SwitchingProtocolsHttpHeadersPart.Length;

            int outputLength =
                partLength +
                accept.Length +
                4 /* \r\n\r\n */;

            // TODO: пока что берем буфер по-умолчанию.
            ByteBuf outByteBuf = ctx.ChannelByteBufAllocator.GetDefault();

            // TODO: optimize all writes
            for (int i = 0; i < partLength; i++)
            {
                outByteBuf.Write(HttpHeaderConstants.SwitchingProtocolsHttpHeadersPart[i]);
            }
            for (int i = 0; i < accept.Length; i++)
            {
                outByteBuf.Write((byte)accept[i]);
            }
            outByteBuf.Write(HttpHeaderConstants.CR);
            outByteBuf.Write(HttpHeaderConstants.LF);
            outByteBuf.Write(HttpHeaderConstants.CR);
            outByteBuf.Write(HttpHeaderConstants.LF);

            return outByteBuf;
        }
    }
}