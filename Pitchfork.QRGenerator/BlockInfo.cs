using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pitchfork.QRGenerator
{
    internal struct BlockInfo
    {
        public readonly int TotalBytes;
        public readonly int DataBytes;

        public BlockInfo(int totalBytes, int dataBytes)
        {
            TotalBytes = totalBytes;
            DataBytes = dataBytes;
        }

        public int ErrorBytes
        {
            get { return TotalBytes - DataBytes; }
        }
    }
}
