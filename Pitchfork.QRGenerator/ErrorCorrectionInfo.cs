using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pitchfork.QRGenerator
{
    internal struct ErrorCorrectionInfo
    {
        public readonly BlockCountInfo[] BlockCountInfos;
        public readonly int TotalDataBytes;
        public readonly int TotalErrorBytes;

        public ErrorCorrectionInfo(params BlockCountInfo[] blockCountInfos)
        {
            BlockCountInfos = blockCountInfos;
            TotalDataBytes = 0;
            TotalErrorBytes = 0;

            checked
            {
                foreach (var blockCountInfo in blockCountInfos)
                {
                    TotalDataBytes += blockCountInfo.BlockCount * blockCountInfo.BlockInfo.DataBytes;
                    TotalErrorBytes += blockCountInfo.BlockCount * blockCountInfo.BlockInfo.ErrorBytes;
                }
            }
        }

        public ErrorCorrectionInfo(int blockCount, int totalBytes, int dataBytes)
            : this(new BlockCountInfo(blockCount, totalBytes, dataBytes)) { }

        public ErrorCorrectionInfo(int blockCountA, int totalBytesA, int dataBytesA, int blockCountB, int totalBytesB, int dataBytesB)
            : this(new BlockCountInfo(blockCountA, totalBytesA, dataBytesA), new BlockCountInfo(blockCountB, totalBytesB, dataBytesB)) { }

        internal int TotalBytes
        {
            get { return checked(TotalDataBytes + TotalErrorBytes); }
        }
    }
}
