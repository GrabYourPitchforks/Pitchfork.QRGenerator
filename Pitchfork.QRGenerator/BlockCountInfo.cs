using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pitchfork.QRGenerator
{
    internal struct BlockCountInfo
    {
        public readonly int BlockCount;
        public readonly BlockInfo BlockInfo;

        public BlockCountInfo(int blockCount, int totalBytes, int dataBytes)
        {
            BlockCount = blockCount;
            BlockInfo = new BlockInfo(totalBytes, dataBytes);
        }
    }
}
