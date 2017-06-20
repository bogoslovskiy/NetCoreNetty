using System.Collections.Generic;
using System.Text;
using NetCoreNetty.Buffers;
using Xunit;

namespace NetCoreNetty.Tests.Buffers
{
    public class SimpleBufferTests
    {
        // ReSharper disable once InconsistentNaming
        static public readonly byte CR = (byte) '\r';
        // ReSharper disable once InconsistentNaming
        static public readonly byte LF = (byte) '\n';

        static public IEnumerable<object[]> Test1Data => new[]
        {
            new object[]
            {
                new[] { "Connection: Upgrade\r\n" },
                4,
                false,
                new[] { 15 }
            },
            new object[]
            {
                new[] { "Connection: Upgrade\r\n" },
                4,
                true,
                new[] { 17 }
            },
            new object[]
            {
                new[] { "Connection: Upgra", "de\r\n" },
                4,
                false,
                new[] { -1, 15 }
            },
            new object[]
            {
                new[] { "Connection: Upgra", "de\r\n" },
                4,
                true,
                new[] { -1, 17 }
            },
            new object[]
            {
                new[] { "Connect", "ion: Upgra", "de\r\n" },
                4,
                false,
                new[] { -1, -1, 15 }
            },
            new object[]
            {
                new[] { "Conne", "ction: Upgra", "de\r\n" },
                4,
                true,
                new[] { -1, -1, 17 }
            }
        };

        [Theory, MemberData(nameof(Test1Data))]
        public void TestByteBufSkip(string[] asciiBuffers, int manualSkip, bool include, int[] expectedSkips)
        {
            SimpleByteBuf byteBuf = null;

            for (int i = 0; i < asciiBuffers.Length; i++)
            {
                byte[] asciiBytes = Encoding.ASCII.GetBytes(asciiBuffers[i]);

                if (byteBuf == null)
                {
                    byteBuf = new SimpleByteBuf(asciiBytes);
                }
                else
                {
                    byteBuf = byteBuf.Concat(asciiBytes);
                }

                if (i == 0)
                {
                    for (int j = 0; j < manualSkip; j++)
                    {
                        byteBuf.ReadByte();
                    }
                }

                int skipped = byteBuf.SkipTo(CR, LF, include);

                Assert.True(skipped == expectedSkips[i]);
            }
        }
    }
}