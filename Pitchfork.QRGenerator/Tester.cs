using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pitchfork.QRGenerator
{
    internal static class Tester
    {
        private static readonly Encoding _iso8859Encoding = Encoding.GetEncoding("ISO-8859-1");
        public static Image GetQRCode(string input, ErrorCorrectionLevel level = ErrorCorrectionLevel.M,  int bordSize = 0, int pixelSize = 6)
        {
            return GetQRCode(_iso8859Encoding.GetBytes(input), level);
        }

        public static Image GetQRCode(byte[] data, ErrorCorrectionLevel level = ErrorCorrectionLevel.M,  int bordSize = 0, int pixelSize = 6)
        {
            bool requires16BitLength = false;
            int maxBytesInVersion9Code = QRErrorCorrections.GetQRVersionInfo(9).GetCorrectionInfo(level).TotalDataBytes;
            if (maxBytesInVersion9Code - 2 < data.Length)
            {
                // This data requires a version 10 or higher code; will not fit in version 9 or lower.
                // Version 10 and higher codes require 16-bit data lengths.
                requires16BitLength = true;
            }

            StreamHelper sh = new StreamHelper();
            sh.WriteNibble(0x04); // byte mode
            if (requires16BitLength)
            {
                sh.WriteWord((ushort)data.Length);
            }
            else
            {
                sh.WriteByte((byte)data.Length);
            }
            sh.WriteBytes(new ArraySegment<byte>(data));
            sh.WriteNibble(0x00); // terminator
            byte[] binaryData = sh.ToArray();

            int qrCodeVersion;
            ErrorCorrectionLevel errorCorrectionLevel;
            byte[] finalMessageSequence = QRErrorCorrections.GetMessageSequence(binaryData, level, out qrCodeVersion, out errorCorrectionLevel);

            SymbolTemplate template = SymbolTemplate.CreateTemplate(qrCodeVersion);
            template.ErrorCorrectionLevel = errorCorrectionLevel;
            template.PopulateData(finalMessageSequence);
            template.Complete();

            return template.ToImage();
        }

        private sealed class StreamHelper
        {
            private readonly MemoryStream _ms = new MemoryStream();
            private byte _bufferBits;
            private int _bufferBitsCount;

            public void WriteByte(byte b)
            {
                WriteBits(b, 8);
            }

            public void WriteBytes(ArraySegment<byte> bytes)
            {
                foreach (var b in bytes)
                {
                    WriteByte(b);
                }
            }

            public void WriteNibble(byte value)
            {
                WriteBits(value, 4);
            }

            public void WriteWord(ushort value)
            {
                WriteBits(value, 16);
            }

            public void WriteBits(uint value, int bits)
            {
                if (bits > 24)
                {
                    WriteBitsImpl(((byte)(value >> 24)), bits - 24);
                }
                if (bits > 16)
                {
                    WriteBitsImpl(((byte)(value >> 16)), Math.Min(8, bits - 16));
                }
                if (bits > 8)
                {
                    WriteBitsImpl(((byte)(value >> 8)), Math.Min(8, bits - 8));
                }
                WriteBitsImpl((byte)value, Math.Min(8, bits));
            }

            private void WriteBitsImpl(byte value, int bits)
            {
                Debug.Assert(0 < bits && bits <= 8);
                if (bits == 0)
                {
                    return;
                }

                var maskedValue = value & (byte)(0xFFU >> (8 - bits));
                var newBufferBits = ((int)_bufferBits) << bits | maskedValue;
                var newBufferBitsCount = _bufferBitsCount + bits;

                if (newBufferBitsCount >= 8)
                {
                    var remainingBits = newBufferBitsCount - 8;
                    byte byteToWrite = (byte)(newBufferBits >> remainingBits);
                    _ms.WriteByte(byteToWrite);
                    _bufferBits = (byte)(newBufferBits & (byte)((1 << remainingBits) - 1));
                    _bufferBitsCount = remainingBits;
                }
                else
                {
                    _bufferBits = (byte)newBufferBits;
                    _bufferBitsCount = newBufferBitsCount;
                }
            }

            internal byte[] ToArray()
            {
                Debug.Assert(_bufferBitsCount == 0);
                return _ms.ToArray();
            }
        }
    }
}
