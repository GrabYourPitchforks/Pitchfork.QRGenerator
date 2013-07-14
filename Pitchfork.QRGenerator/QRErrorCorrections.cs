using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pitchfork.QRGenerator
{
    internal static class QRErrorCorrections
    {
        private static readonly QRVersion[] _versions = GetVersions();

        private static QRVersion[] GetVersions()
        {
            // See ISO/IEC 18004:2006(E), Table 9
            QRVersion[] versions = new QRVersion[40];

            // Version 1
            versions[0] = new QRVersion(
                l: new ErrorCorrectionInfo(1, 26, 19),
                m: new ErrorCorrectionInfo(1, 26, 16),
                q: new ErrorCorrectionInfo(1, 26, 13),
                h: new ErrorCorrectionInfo(1, 26, 9));

            // Version 2
            versions[1] = new QRVersion(
                l: new ErrorCorrectionInfo(1, 44, 34),
                m: new ErrorCorrectionInfo(1, 44, 28),
                q: new ErrorCorrectionInfo(1, 44, 22),
                h: new ErrorCorrectionInfo(1, 44, 16));

            // Version 3
            versions[2] = new QRVersion(
                l: new ErrorCorrectionInfo(1, 70, 55),
                m: new ErrorCorrectionInfo(1, 70, 44),
                q: new ErrorCorrectionInfo(2, 35, 17),
                h: new ErrorCorrectionInfo(2, 35, 13));

            // Version 4
            versions[3] = new QRVersion(
                 l: new ErrorCorrectionInfo(1, 100, 80),
                 m: new ErrorCorrectionInfo(2, 50, 32),
                 q: new ErrorCorrectionInfo(2, 50, 24),
                 h: new ErrorCorrectionInfo(4, 25, 9));

            // Version 5
            versions[4] = new QRVersion(
                 l: new ErrorCorrectionInfo(1, 134, 108),
                 m: new ErrorCorrectionInfo(2, 67, 43),
                 q: new ErrorCorrectionInfo(2, 33, 15, 2, 34, 16),
                 h: new ErrorCorrectionInfo(2, 33, 11, 2, 34, 12));

            // Version 6
            versions[5] = new QRVersion(
                 l: new ErrorCorrectionInfo(2, 86, 68),
                 m: new ErrorCorrectionInfo(4, 43, 27),
                 q: new ErrorCorrectionInfo(4, 43, 19),
                 h: new ErrorCorrectionInfo(4, 43, 15));

            // Version 7
            versions[6] = new QRVersion(
                 l: new ErrorCorrectionInfo(2, 98, 78),
                 m: new ErrorCorrectionInfo(4, 49, 31),
                 q: new ErrorCorrectionInfo(2, 32, 14, 4, 33, 15),
                 h: new ErrorCorrectionInfo(4, 39, 13, 1, 40, 14));

            // Version 8
            versions[7] = new QRVersion(
                 l: new ErrorCorrectionInfo(2, 121, 97),
                 m: new ErrorCorrectionInfo(2, 60, 38, 2, 61, 39),
                 q: new ErrorCorrectionInfo(4, 40, 18, 2, 41, 19),
                 h: new ErrorCorrectionInfo(4, 40, 14, 2, 41, 15));

            // Version 9
            versions[8] = new QRVersion(
                l: new ErrorCorrectionInfo(2, 146, 116),
                m: new ErrorCorrectionInfo(3, 58, 36, 2, 59, 37),
                q: new ErrorCorrectionInfo(4, 36, 16, 4, 37, 17),
                h: new ErrorCorrectionInfo(4, 36, 12, 4, 37, 13));

            // Version 10
            versions[9] = new QRVersion(
                l: new ErrorCorrectionInfo(2, 86, 68, 2, 87, 69),
                m: new ErrorCorrectionInfo(4, 69, 43, 1, 70, 44),
                q: new ErrorCorrectionInfo(6, 43, 19, 2, 44, 20),
                h: new ErrorCorrectionInfo(6, 43, 15, 2, 44, 16));

            // Version 11
            versions[10] = new QRVersion(
                l: new ErrorCorrectionInfo(4, 101, 81),
                m: new ErrorCorrectionInfo(1, 80, 50, 4, 81, 51),
                q: new ErrorCorrectionInfo(4, 50, 22, 4, 51, 23),
                h: new ErrorCorrectionInfo(3, 36, 12, 8, 37, 13));

            // Version 12
            versions[11] = new QRVersion(
                l: new ErrorCorrectionInfo(2, 116, 92, 2, 117, 93),
                m: new ErrorCorrectionInfo(6, 58, 36, 2, 59, 37),
                q: new ErrorCorrectionInfo(4, 46, 20, 6, 47, 21),
                h: new ErrorCorrectionInfo(7, 42, 14, 4, 43, 15));

            // Version 13
            versions[12] = new QRVersion(
                l: new ErrorCorrectionInfo(2, 116, 92, 2, 117, 93),
                m: new ErrorCorrectionInfo(6, 58, 36, 2, 59, 37),
                q: new ErrorCorrectionInfo(4, 46, 20, 6, 47, 21),
                h: new ErrorCorrectionInfo(7, 42, 14, 4, 43, 15));

            // Version 14
            versions[13] = new QRVersion(
                l: new ErrorCorrectionInfo(3, 145, 115, 1, 146, 116),
                m: new ErrorCorrectionInfo(4, 64, 40, 5, 65, 41),
                q: new ErrorCorrectionInfo(11, 36, 16, 5, 37, 17),
                h: new ErrorCorrectionInfo(11, 36, 12, 5, 37, 13));

            // Version 15
            versions[14] = new QRVersion(
                l: new ErrorCorrectionInfo(5, 109, 87, 1, 110, 88),
                m: new ErrorCorrectionInfo(5, 65, 41, 5, 66, 42),
                q: new ErrorCorrectionInfo(5, 54, 24, 7, 55, 25),
                h: new ErrorCorrectionInfo(11, 36, 12, 7, 37, 13));

            // Version 16
            versions[15] = new QRVersion(
                l: new ErrorCorrectionInfo(5, 122, 98, 1, 123, 99),
                m: new ErrorCorrectionInfo(7, 73, 45, 3, 74, 46),
                q: new ErrorCorrectionInfo(15, 43, 19, 2, 44, 20),
                h: new ErrorCorrectionInfo(3, 45, 15, 13, 46, 16));

            // Version 17
            versions[16] = new QRVersion(
                l: new ErrorCorrectionInfo(1, 135, 107, 5, 136, 108),
                m: new ErrorCorrectionInfo(10, 74, 46, 1, 75, 47),
                q: new ErrorCorrectionInfo(1, 50, 22, 15, 51, 23),
                h: new ErrorCorrectionInfo(2, 42, 14, 17, 43, 15));

            // Version 18
            versions[17] = new QRVersion(
                l: new ErrorCorrectionInfo(5, 150, 120, 1, 151, 121),
                m: new ErrorCorrectionInfo(9, 69, 43, 4, 70, 44),
                q: new ErrorCorrectionInfo(17, 50, 22, 1, 51, 23),
                h: new ErrorCorrectionInfo(2, 42, 14, 19, 43, 15));

            // Version 19
            versions[18] = new QRVersion(
                l: new ErrorCorrectionInfo(3, 141, 113, 4, 142, 114),
                m: new ErrorCorrectionInfo(3, 70, 44, 11, 71, 45),
                q: new ErrorCorrectionInfo(17, 47, 21, 4, 48, 22),
                h: new ErrorCorrectionInfo(9, 39, 13, 16, 40, 14));

            // Version 20
            versions[19] = new QRVersion(
                l: new ErrorCorrectionInfo(3, 135, 107, 5, 136, 108),
                m: new ErrorCorrectionInfo(3, 67, 41, 13, 68, 42),
                q: new ErrorCorrectionInfo(15, 54, 24, 5, 55, 25),
                h: new ErrorCorrectionInfo(15, 43, 15, 10, 44, 16));

            // Version 21
            versions[20] = new QRVersion(
                l: new ErrorCorrectionInfo(4, 144, 116, 4, 145, 117),
                m: new ErrorCorrectionInfo(17, 68, 42),
                q: new ErrorCorrectionInfo(17, 50, 22, 6, 51, 23),
                h: new ErrorCorrectionInfo(19, 46, 16, 6, 47, 17));

            // Version 21
            versions[21] = new QRVersion(
                l: new ErrorCorrectionInfo(2, 139, 111, 7, 140, 112),
                m: new ErrorCorrectionInfo(17, 74, 46),
                q: new ErrorCorrectionInfo(7, 54, 24, 16, 55, 25),
                h: new ErrorCorrectionInfo(34, 37, 13));

            // Version 23
            versions[22] = new QRVersion(
                l: new ErrorCorrectionInfo(4, 151, 121, 5, 152, 122),
                m: new ErrorCorrectionInfo(4, 75, 47, 14, 76, 48),
                q: new ErrorCorrectionInfo(11, 54, 24, 14, 55, 25),
                h: new ErrorCorrectionInfo(16, 45, 15, 14, 46, 16));

            // Version 24
            versions[23] = new QRVersion(
                l: new ErrorCorrectionInfo(6, 147, 117, 4, 148, 118),
                m: new ErrorCorrectionInfo(6, 73, 45, 14, 74, 46),
                q: new ErrorCorrectionInfo(11, 54, 24, 16, 55, 25),
                h: new ErrorCorrectionInfo(30, 46, 16, 2, 47, 17));

            // Version 25
            versions[24] = new QRVersion(
                l: new ErrorCorrectionInfo(8, 132, 106, 4, 133, 107),
                m: new ErrorCorrectionInfo(8, 75, 47, 13, 76, 48),
                q: new ErrorCorrectionInfo(7, 54, 24, 22, 55, 25),
                h: new ErrorCorrectionInfo(22, 45, 15, 13, 46, 16));

            // Version 26
            versions[25] = new QRVersion(
                l: new ErrorCorrectionInfo(10, 142, 114, 2, 143, 115),
                m: new ErrorCorrectionInfo(19, 74, 46, 4, 75, 47),
                q: new ErrorCorrectionInfo(28, 50, 22, 6, 51, 23),
                h: new ErrorCorrectionInfo(33, 46, 16, 4, 47, 17));

            // Version 27
            versions[26] = new QRVersion(
                l: new ErrorCorrectionInfo(8, 152, 122, 4, 153, 123),
                m: new ErrorCorrectionInfo(22, 73, 45, 3, 74, 46),
                q: new ErrorCorrectionInfo(8, 53, 23, 26, 54, 24),
                h: new ErrorCorrectionInfo(12, 45, 15, 28, 46, 16));

            // Version 28
            versions[27] = new QRVersion(
                l: new ErrorCorrectionInfo(3, 147, 117, 10, 148, 118),
                m: new ErrorCorrectionInfo(3, 73, 45, 23, 74, 46),
                q: new ErrorCorrectionInfo(4, 54, 24, 31, 55, 25),
                h: new ErrorCorrectionInfo(11, 45, 15, 31, 46, 16));

            // Version 29
            versions[28] = new QRVersion(
                l: new ErrorCorrectionInfo(7, 146, 116, 7, 147, 117),
                m: new ErrorCorrectionInfo(21, 73, 45, 7, 74, 46),
                q: new ErrorCorrectionInfo(1, 53, 23, 37, 54, 24),
                h: new ErrorCorrectionInfo(19, 45, 15, 26, 46, 16));

            // Version 30
            versions[29] = new QRVersion(
                l: new ErrorCorrectionInfo(5, 145, 115, 10, 146, 116),
                m: new ErrorCorrectionInfo(19, 75, 47, 10, 76, 48),
                q: new ErrorCorrectionInfo(15, 54, 24, 25, 55, 25),
                h: new ErrorCorrectionInfo(23, 45, 15, 25, 46, 16));

            // Version 31
            versions[30] = new QRVersion(
                l: new ErrorCorrectionInfo(13, 145, 115, 3, 146, 116),
                m: new ErrorCorrectionInfo(2, 74, 46, 29, 75, 47),
                q: new ErrorCorrectionInfo(42, 54, 24, 1, 55, 25),
                h: new ErrorCorrectionInfo(23, 45, 15, 28, 46, 16));

            // Version 32
            versions[31] = new QRVersion(
                l: new ErrorCorrectionInfo(17, 145, 115),
                m: new ErrorCorrectionInfo(10, 74, 46, 23, 75, 47),
                q: new ErrorCorrectionInfo(10, 54, 24, 35, 55, 25),
                h: new ErrorCorrectionInfo(19, 45, 15, 35, 46, 16));

            // Version 33
            versions[32] = new QRVersion(
                l: new ErrorCorrectionInfo(17, 145, 115, 1, 146, 116),
                m: new ErrorCorrectionInfo(14, 74, 46, 21, 75, 47),
                q: new ErrorCorrectionInfo(29, 54, 24, 19, 55, 25),
                h: new ErrorCorrectionInfo(11, 45, 15, 46, 46, 16));

            // Version 34
            versions[33] = new QRVersion(
                l: new ErrorCorrectionInfo(13, 145, 115, 6, 146, 116),
                m: new ErrorCorrectionInfo(14, 74, 46, 23, 75, 47),
                q: new ErrorCorrectionInfo(44, 54, 24, 7, 55, 25),
                h: new ErrorCorrectionInfo(59, 46, 16, 1, 47, 17));

            // Version 15
            versions[34] = new QRVersion(
                l: new ErrorCorrectionInfo(12, 151, 121, 7, 152, 122),
                m: new ErrorCorrectionInfo(12, 75, 47, 26, 76, 48),
                q: new ErrorCorrectionInfo(39, 54, 24, 14, 55, 25),
                h: new ErrorCorrectionInfo(22, 45, 15, 41, 46, 16));

            // Version 36
            versions[35] = new QRVersion(
                l: new ErrorCorrectionInfo(6, 151, 121, 14, 152, 122),
                m: new ErrorCorrectionInfo(6, 75, 47, 34, 76, 48),
                q: new ErrorCorrectionInfo(46, 54, 24, 10, 55, 25),
                h: new ErrorCorrectionInfo(2, 45, 15, 64, 46, 16));

            // Version 37
            versions[36] = new QRVersion(
                l: new ErrorCorrectionInfo(17, 152, 122, 4, 153, 123),
                m: new ErrorCorrectionInfo(29, 74, 46, 14, 75, 47),
                q: new ErrorCorrectionInfo(49, 54, 24, 10, 55, 25),
                h: new ErrorCorrectionInfo(24, 45, 15, 46, 46, 16));

            // Version 38
            versions[37] = new QRVersion(
                l: new ErrorCorrectionInfo(4, 152, 122, 18, 153, 123),
                m: new ErrorCorrectionInfo(13, 74, 46, 32, 75, 47),
                q: new ErrorCorrectionInfo(48, 54, 24, 14, 55, 25),
                h: new ErrorCorrectionInfo(42, 45, 15, 32, 46, 16));

            // Version 39
            versions[38] = new QRVersion(
                l: new ErrorCorrectionInfo(20, 147, 117, 4, 148, 118),
                m: new ErrorCorrectionInfo(40, 75, 47, 7, 76, 48),
                q: new ErrorCorrectionInfo(43, 54, 24, 22, 55, 25),
                h: new ErrorCorrectionInfo(10, 45, 15, 67, 46, 16));

            // Version 40
            versions[39] = new QRVersion(
                l: new ErrorCorrectionInfo(19, 148, 118, 6, 149, 119),
                m: new ErrorCorrectionInfo(18, 75, 47, 31, 76, 48),
                q: new ErrorCorrectionInfo(34, 54, 24, 34, 55, 25),
                h: new ErrorCorrectionInfo(20, 45, 15, 61, 46, 16));

            return versions;
        }

        internal static QRVersion GetQRVersionInfo(int version)
        {
            Debug.Assert(1 <= version && version < _versions.Length);
            return _versions[version - 1];
        }

        internal static ErrorCorrectionInfo GetErrorCorrectionInfo(int inputBytes, ErrorCorrectionLevel desiredLevel, out int qrCodeVersion, out ErrorCorrectionLevel qrErrorCorrectionLevel)
        {
            for (int i = 0; i < _versions.Length; i++)
            {
                QRVersion version = _versions[i];
                if (version.GetCorrectionInfo(desiredLevel).TotalDataBytes >= inputBytes)
                {
                    // Found match, now get highest correction code within this version that can still hold desired number of bytes.
                    qrCodeVersion = i + 1;
                    if (version.H.TotalDataBytes >= inputBytes) { qrErrorCorrectionLevel = ErrorCorrectionLevel.H; return version.H; }
                    else if (version.Q.TotalDataBytes >= inputBytes) { qrErrorCorrectionLevel = ErrorCorrectionLevel.Q; return version.Q; }
                    else if (version.M.TotalDataBytes >= inputBytes) { qrErrorCorrectionLevel = ErrorCorrectionLevel.M; return version.M; }
                    else { qrErrorCorrectionLevel = ErrorCorrectionLevel.L; return version.L; }
                }
            }

            throw new InputTooLongException();
        }

        internal static byte[] GetMessageSequence(byte[] input, ErrorCorrectionLevel desiredLevel, out int qrCodeVersion, out ErrorCorrectionLevel qrErrorCorrectionLevel)
        {
            // See ISO/IEC 18004:2006(E), Sec. 6.6 for information on generating the final message sequence

            ErrorCorrectionInfo correctionInfo = GetErrorCorrectionInfo(input.Length, desiredLevel, out qrCodeVersion, out qrErrorCorrectionLevel);
            byte[] extendedInput = new byte[correctionInfo.TotalDataBytes];
            Buffer.BlockCopy(input, 0, extendedInput, 0, input.Length);

            // generate data and error blocks
            int bytesCopiedSoFar = 0;
            List<byte[]> dataBlocks = new List<byte[]>();
            List<byte[]> errorBlocks = new List<byte[]>();
            foreach (var blockCountInfo in correctionInfo.BlockCountInfos)
            {
                for (int i = 0; i < blockCountInfo.BlockCount; i++)
                {
                    // create a data block
                    byte[] thisDataBlock = new byte[blockCountInfo.BlockInfo.DataBytes];
                    Buffer.BlockCopy(extendedInput, bytesCopiedSoFar, thisDataBlock, 0, thisDataBlock.Length);
                    dataBlocks.Add(thisDataBlock);
                    bytesCopiedSoFar += thisDataBlock.Length;

                    // create an error block
                    byte[] thisErrorBlock = QRReedSolomon.GetErrorCorrectionBytes(thisDataBlock, blockCountInfo.BlockInfo.ErrorBytes);
                    errorBlocks.Add(thisErrorBlock);
                }
            }

            // Assemble the final message sequence
            Debug.Assert(dataBlocks.Count == errorBlocks.Count);
            MemoryStream ms = new MemoryStream(correctionInfo.TotalDataBytes + correctionInfo.TotalErrorBytes);
            FlushBlocks(dataBlocks, ms);
            FlushBlocks(errorBlocks, ms);
            return ms.ToArray();
        }

        private static void FlushBlocks(List<byte[]> blocks, MemoryStream stream)
        {
            for (int i = 0; ; i++)
            {
                bool wroteBlock = false;
                for (int j = 0; j < blocks.Count; j++)
                {
                    byte[] thisBlock = blocks[j];
                    if (thisBlock.Length > i)
                    {
                        wroteBlock = true;
                        stream.WriteByte(thisBlock[i]);
                    }
                }
                if (!wroteBlock)
                {
                    return;
                }
            }
        }
    }
}
