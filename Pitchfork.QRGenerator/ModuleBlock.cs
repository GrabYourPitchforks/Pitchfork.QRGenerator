using System;
using System.Diagnostics;

namespace Pitchfork.QRGenerator
{
    internal sealed class ModuleBlock
    {
        private readonly byte[] _buffer;
        private readonly int _width;

        public ModuleBlock(int width)
        {
            Debug.Assert(0 <= width);
            _buffer = new byte[checked((width * width + 3) / 4)]; // CEIL(width * width / 4)
            _width = width;
        }

        // copy ctor
        private ModuleBlock(ModuleBlock original)
        {
            this._buffer = (byte[])(original._buffer.Clone());
            this._width = original._width;
        }

        private void CheckParameters(int row, int col)
        {
            Debug.Assert(0 <= row && row < _width);
            Debug.Assert(0 <= col && col < _width);
        }

        internal ModuleBlock CreateCopy()
        {
            return new ModuleBlock(this);
        }

        public void FlipColor(int row, int col)
        {
            CheckParameters(row, col);

            int blockNum, blockOffset;
            GetOffsets(row, col, out blockNum, out blockOffset);
            _buffer[blockNum] ^= (byte)(0x1 << (2 * blockOffset));
        }

        public bool IsDark(int row, int col)
        {
            CheckParameters(row, col);
            return ((GetFlag(row, col) & (ModuleFlag.Light | ModuleFlag.Dark)) == ModuleFlag.Dark);
        }

        public bool IsLight(int row, int col)
        {
            CheckParameters(row, col);
            return ((GetFlag(row, col) & (ModuleFlag.Light | ModuleFlag.Dark)) == ModuleFlag.Light);
        }

        public bool IsReserved(int row, int col)
        {
            CheckParameters(row, col);
            return ((GetFlag(row, col) & ModuleFlag.Reserved) == ModuleFlag.Reserved);
        }

        public void Set(int row, int col, ModuleFlag flag, bool overwrite = false)
        {
            CheckParameters(row, col);
            SetFlag(row, col, flag, overwrite);
        }

        private ModuleFlag GetFlag(int row, int col)
        {
            int blockNum, blockOffset;
            GetOffsets(row, col, out blockNum, out blockOffset);
            return (ModuleFlag)((_buffer[blockNum] >> (2 * blockOffset)) & 0x3);
        }

        private void SetFlag(int row, int col, ModuleFlag flag, bool overwrite)
        {
            int blockNum, blockOffset;
            GetOffsets(row, col, out blockNum, out blockOffset);
            int leftShiftValue = 2 * blockOffset;
            byte newFlag = (byte)(((byte)flag & 0x3) << leftShiftValue);
            if (overwrite)
            {
                byte originalValue = _buffer[blockNum];
                byte originalValueMasked = (byte)(originalValue & ~(0x3 << leftShiftValue));
                byte newValue = (byte)(originalValueMasked | newFlag);
                _buffer[blockNum] = newValue;
            }
            else
            {
                _buffer[blockNum] |= newFlag;
            }
        }

        private void GetOffsets(int row, int col, out int blockNum, out int blockOffset)
        {
            int elementPos = row * _width + col;
            blockNum = elementPos / 4;
            blockOffset = elementPos % 4;
        }
    }
}
