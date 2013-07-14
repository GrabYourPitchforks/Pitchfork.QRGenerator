using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pitchfork.QRGenerator
{
    internal sealed partial class SymbolTemplate
    {
        private static readonly SymbolTemplate[] _symbolTemplates = new SymbolTemplate[40];
        private static readonly uint[] _formatInformationEncodings = GetFormatInformationEncodings();
        private static readonly uint[] _versionInformationEncodings = GetVersionInformationEncodings();

        private ModuleBlock _modules;

        private SymbolTemplate(int version)
        {
            // See ISO/IEC 18004:2006(E), Sec. 5.3.1.1 for how versions map to sizes.
            Debug.Assert(1 <= version && version <= 40);
            Version = version;

            Width = 21 + 4 * (version - 1);
            _modules = new ModuleBlock(Width);
            PopulateReservedModuleBits();
        }

        // copy ctor
        private SymbolTemplate(SymbolTemplate prototype)
        {
            this._modules = prototype._modules.CreateCopy();
            this.ErrorCorrectionLevel = prototype.ErrorCorrectionLevel;
            this.Version = prototype.Version;
            this.Width = prototype.Width;
        }

        public int Version { get; private set; }
        public int Width { get; private set; }
        public ErrorCorrectionLevel ErrorCorrectionLevel { get; set; }

        private void PopulateReservedModuleBits()
        {
            // This isn't the most efficient way to fill in the template, but it's only
            // performed once, then cached. So speed isn't a huge issue.

            // Fill in timing pattern in row 6 and col 6.
            // ISO/IEC 18004:2006(E), Sec. 5.3.4
            for (int i = 0; i < Width; i++)
            {
                ModuleFlag moduleFlag = ModuleFlag.Reserved | ((i % 2 == 0) ? ModuleFlag.Dark : ModuleFlag.Light);
                _modules.Set(i, 6, moduleFlag, overwrite: true);
                _modules.Set(6, i, moduleFlag, overwrite: true);
            }

            // Fill in the finder patterns (the "QR" patterns)
            PopulateFinderPatterns();

            // Fill in alignment patterns (the smaller "QR" patterns throughout the encoding region)
            PopulateAlignmentPatterns();

            // Fill in version information (next to the finder pattern)
            PopulateVersionInformation();
        }

        private void PopulateFinderPatterns()
        {
            // Finder pattern is a 3x3 dark square inscribed within a 5x5 light square inscribed within a 7x7 dark square.
            // The finder pattern is separated from the rest of the data section by a one-module width line.
            // See ISO/IEC 18004:2006(E), Sec. 5.3.2.1 and Sec. 5.3.3

            // Top-left
            DrawSquare(_modules, 0, 0, 9, ModuleFlag.Reserved, xor: true); // reserved for format information
            DrawSquare(_modules, 0, 0, 8, ModuleFlag.Reserved | ModuleFlag.Light); // separator
            DrawSquare(_modules, 0, 0, 7, ModuleFlag.Reserved | ModuleFlag.Dark);
            DrawSquare(_modules, 1, 1, 5, ModuleFlag.Reserved | ModuleFlag.Light);
            DrawSquare(_modules, 2, 2, 3, ModuleFlag.Reserved | ModuleFlag.Dark);

            // Top-right
            DrawRect(_modules, 8, Width - 8, 8, 1, ModuleFlag.Reserved, xor: true); // reserved for format information
            DrawSquare(_modules, 0, Width - 8, 8, ModuleFlag.Reserved | ModuleFlag.Light); // separator
            DrawSquare(_modules, 0, Width - 7, 7, ModuleFlag.Reserved | ModuleFlag.Dark);
            DrawSquare(_modules, 1, Width - 6, 5, ModuleFlag.Reserved | ModuleFlag.Light);
            DrawSquare(_modules, 2, Width - 5, 3, ModuleFlag.Reserved | ModuleFlag.Dark);

            // Bottom-left
            DrawRect(_modules, Width - 8, 8, 1, 8, ModuleFlag.Reserved, xor: true); // reserved for format information
            DrawSquare(_modules, Width - 8, 0, 8, ModuleFlag.Reserved | ModuleFlag.Light); // separator
            DrawSquare(_modules, Width - 7, 0, 7, ModuleFlag.Reserved | ModuleFlag.Dark);
            DrawSquare(_modules, Width - 6, 1, 5, ModuleFlag.Reserved | ModuleFlag.Light);
            DrawSquare(_modules, Width - 5, 2, 3, ModuleFlag.Reserved | ModuleFlag.Dark);
        }

        private void PopulateAlignmentPatterns()
        {
            // Alignment pattern is a 1x1 dark square inscribed within a 3x3 light square inscribed within a 5x5 dark square.
            // See ISO/IEC 18004:2006(E), Sec. 5.3.5
            foreach (var coord in AlignmentPatternUtil.GetCoords(Version))
            {
                DrawSquare(_modules, coord.Row - 2, coord.Col - 2, 5, ModuleFlag.Reserved | ModuleFlag.Dark);
                DrawSquare(_modules, coord.Row - 1, coord.Col - 1, 3, ModuleFlag.Reserved | ModuleFlag.Light);
                DrawSquare(_modules, coord.Row, coord.Col, 1, ModuleFlag.Reserved | ModuleFlag.Dark);
            }
        }

        private void PopulateVersionInformation()
        {
            // Version information appears next to the QR finder pattern
            // See ISO/IEC 18004:2006(E), Sec. 6.10

            // Pattern only appears in QR code version 7 and greater
            if (Version < 7) { return; }

            // The version information is an 18-bit sequence: the 6-bit version number and a 12-bit error correction code
            uint versionInformation = _versionInformationEncodings[Version - 7];
            Debug.Assert(versionInformation >> 12 == Version);

            for (int i = 0; i < 18; i++)
            {
                ModuleFlag type = ModuleFlag.Reserved | ((((versionInformation >> i) & 0x2) != 0) ? ModuleFlag.Dark : ModuleFlag.Light);
                int x = i / 3;
                int y = Width - 11 + (i % 3);
                _modules.Set(x, y, type, overwrite: true); // upper-right
                _modules.Set(y, x, type, overwrite: true); // lower-left
            }
        }

        // Draws a square of the specified width whose top-left corner is at the specified coords
        private static void DrawSquare(ModuleBlock modules, int row, int col, int width, ModuleFlag type, bool xor = false)
        {
            DrawRect(modules, row, col, width, width, type, xor);
        }

        private static void DrawRect(ModuleBlock modules, int row, int col, int width, int height, ModuleFlag type, bool xor = false)
        {
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    modules.Set(row + i, col + j, type, !xor);
                }
            }
        }

        private static uint[] GetVersionInformationEncodings()
        {
            // Version information encodings are given in ISO/IEC 18004:2006(E), Annex D, Table D.1
            return new uint[] {
                0x07C94, // version 7
                0x085BC, // version 8
                0x09A99, // ...
                0x0A4D3,
                0x0BBF6,
                0x0C762,
                0x0D847,
                0x0E60D,
                0x0F928,
                0x10B78,
                0x1145D,
                0x12A17,
                0x13532,
                0x149A6,
                0x15683,
                0x168C9,
                0x177EC,
                0x18EC4,
                0x191E1,
                0x1AFAB,
                0x1B08E,
                0x1CC1A,
                0x1D33F,
                0x1ED75,
                0x1F250,
                0x209D5,
                0x216F0,
                0x228BA,
                0x2379F,
                0x24B0B,
                0x2542E,
                0x26A64,
                0x27541,
                0x28C69, // version 40
            };
        }

        private static uint[] GetFormatInformationEncodings()
        {
            // Format information encodings are given in ISO/IEC 18004:2006(E), Annex C, Table C.1
            return new uint[] {
                0x5412, // data = 00000
                0x5125, // data = 00001
                0x5E7C, // data = 00010
                0x5B4B, // ...
                0x45F9, 
                0x40CE, 
                0x4F97, 
                0x4AA0, 
                0x77C4, 
                0x72F3, 
                0x7DAA, 
                0x789D, 
                0x662F, 
                0x6318, 
                0x6C41, 
                0x6976, 
                0x1689, 
                0x13BE, 
                0x1CE7, 
                0x19D0, 
                0x0762, 
                0x0255, 
                0x0D0C, 
                0x083B, 
                0x355F, 
                0x3068, 
                0x3F31, 
                0x3A06, 
                0x24B4, 
                0x2183, 
                0x2EDA, 
                0x2BED, // data = 11111
            };
        }

        public static SymbolTemplate CreateTemplate(int version)
        {
            Debug.Assert(1 <= version && version <= 40);

            // Lazy initialization of this symbol template
            SymbolTemplate template = Volatile.Read(ref _symbolTemplates[version - 1]);
            if (template == null)
            {
                SymbolTemplate newTemplate = new SymbolTemplate(version);
                template = Interlocked.CompareExchange(ref _symbolTemplates[version - 1], newTemplate, null) ?? newTemplate;
            }

            // Return a copy since the templates are mutable
            return new SymbolTemplate(template);
        }

        // Applies the masking pattern and finishes the template
        public void Complete()
        {
            uint maskingPattern = SelectAndApplyMaskingPattern();
            uint formatInformationDataBits = GetBinaryIndicatorForErrorCorrectionLevel(ErrorCorrectionLevel) << 3 | maskingPattern;
            uint formatInformationEncodedBits = _formatInformationEncodings[formatInformationDataBits];
            ApplyFormatInformationEncodedBits(formatInformationEncodedBits);
        }

        public Image ToImage()
        {
            const int PIXELSIZE = 6;
            const int BORDER = 4;

            int pixelWidth = (Width + BORDER * 2) * PIXELSIZE;
            Bitmap bmp = new Bitmap(pixelWidth, pixelWidth);

            using (var graphics = Graphics.FromImage(bmp))
            {
                // Everything is white by default
                graphics.FillRectangle(Brushes.White, 0, 0, pixelWidth, pixelWidth);

                // Set the black areas
                for (int row = 0; row < Width; row++)
                {
                    for (int col = 0; col < Width; col++)
                    {
                        if (_modules.IsDark(row,col))
                        {
                            graphics.FillRectangle(Brushes.Black, (col + BORDER) * PIXELSIZE, (row + BORDER) * PIXELSIZE, PIXELSIZE, PIXELSIZE);
                        }
                    }
                }
            }

            return bmp;
        }

        private void ApplyFormatInformationEncodedBits(uint bits)
        {
            // See ISO/IEC 18004:2006(E), Sec. 6.9.1, Figure 25

            ModuleFlag b14 = GetTypeFromBitPosition(bits, 14);
            _modules.Set(8, 0, b14);
            _modules.Set(Width - 1, 8, b14);

            ModuleFlag b13 = GetTypeFromBitPosition(bits, 13);
            _modules.Set(8, 1, b13);
            _modules.Set(Width - 2, 8, b13);

            ModuleFlag b12 = GetTypeFromBitPosition(bits, 12);
            _modules.Set(8, 2, b12);
            _modules.Set(Width - 3, 8, b12);

            ModuleFlag b11 = GetTypeFromBitPosition(bits, 11);
            _modules.Set(8, 3, b11);
            _modules.Set(Width - 4, 8, b11);

            ModuleFlag b10 = GetTypeFromBitPosition(bits, 10);
            _modules.Set(8, 4, b10);
            _modules.Set(Width - 5, 8, b10);

            ModuleFlag b09 = GetTypeFromBitPosition(bits, 9);
            _modules.Set(8, 5, b09);
            _modules.Set(Width - 6, 8, b09);

            ModuleFlag b08 = GetTypeFromBitPosition(bits, 8);
            _modules.Set(8, 7, b08);
            _modules.Set(Width - 7, 8, b08);
            _modules.Set(Width - 8, 8, ModuleFlag.Dark); // dark module above bit 8 in bottom-left

            ModuleFlag b07 = GetTypeFromBitPosition(bits, 7);
            _modules.Set(8, 8, b07);
            _modules.Set(8, Width - 8, b07);

            ModuleFlag b06 = GetTypeFromBitPosition(bits, 6);
            _modules.Set(7, 8, b06);
            _modules.Set(8, Width - 7, b06);

            ModuleFlag b05 = GetTypeFromBitPosition(bits, 5);
            _modules.Set(5, 8, b05);
            _modules.Set(8, Width - 6, b05);

            ModuleFlag b04 = GetTypeFromBitPosition(bits, 4);
            _modules.Set(4, 8, b04);
            _modules.Set(8, Width - 5, b04);

            ModuleFlag b03 = GetTypeFromBitPosition(bits, 3);
            _modules.Set(3, 8, b03);
            _modules.Set(8, Width - 4, b03);

            ModuleFlag b02 = GetTypeFromBitPosition(bits, 2);
            _modules.Set(2, 8, b02);
            _modules.Set(8, Width - 3, b02);

            ModuleFlag b01 = GetTypeFromBitPosition(bits, 1);
            _modules.Set(1, 8, b01);
            _modules.Set(8, Width - 2, b01);

            ModuleFlag b00 = GetTypeFromBitPosition(bits, 0);
            _modules.Set(0, 8, b00);
            _modules.Set(8, Width - 1, b00);
        }

        private static ModuleFlag GetTypeFromBitPosition(uint value, int bitPosition)
        {
            return (((value >> bitPosition) & 0x1) != 0) ? ModuleFlag.Dark : ModuleFlag.Light;
        }

        private static uint GetBinaryIndicatorForErrorCorrectionLevel(ErrorCorrectionLevel level)
        {
            // See ISO/IEC 18004:2006(E), Sec. 6.9.1, Table 12
            switch (level)
            {
                case ErrorCorrectionLevel.L: return 1;
                case ErrorCorrectionLevel.M: return 0;
                case ErrorCorrectionLevel.Q: return 3;
                case ErrorCorrectionLevel.H: return 2;
            }

            throw new InvalidOperationException();
        }

        private uint SelectAndApplyMaskingPattern()
        {
            // Masks the QR code to minimize chance of data transmission error
            // See ISO/IEC 18004:2006(E), Sec. 6.8

            SymbolTemplate[] candidates = new SymbolTemplate[_dataMaskPredicates.Length - 1];
            for (int i = 0; i < candidates.Length; i++)
            {
                SymbolTemplate candidate = new SymbolTemplate(this); // copy ctor
                candidate.ApplyMask(_dataMaskPredicates[i]);
                candidates[i] = candidate;
            }

            int lowestPenaltyScoreValue = candidates[0].CalculatePenaltyScore();
            int lowestPenaltyScoreIndex = 0;
            for (int i = 1; i < candidates.Length; i++)
            {
                int candidatePenaltyScoreValue = candidates[i].CalculatePenaltyScore();
                if (candidatePenaltyScoreValue < lowestPenaltyScoreValue)
                {
                    lowestPenaltyScoreValue = candidatePenaltyScoreValue;
                    lowestPenaltyScoreIndex = i;
                }
            }

            // copy the candidate modules array into me; copy by ref is fine
            this._modules = candidates[lowestPenaltyScoreIndex]._modules;
            return (uint)lowestPenaltyScoreIndex;
        }

        private void ApplyMask(DataMaskPredicate predicate)
        {
            for (int row = 0; row < Width; row++)
            {
                for (int col = 0; col < Width; col++)
                {
                    if (!_modules.IsReserved(row, col) && predicate(row, col))
                    {
                        _modules.FlipColor(row, col);
                    }
                }
            }
        }

        internal bool IsReservedModule(int row, int col)
        {
            return _modules.IsReserved(row, col);
        }

        public string DumpDebugString()
        {
            return DumpDebugStringImpl(_modules.IsDark);
        }

        public string DumpReservedString()
        {
            return DumpDebugStringImpl(_modules.IsReserved);
        }

        private string DumpDebugStringImpl(Func<int, int, bool> predicate)
        {
            StringBuilder sb = new StringBuilder();
            for (int row = 0; row < Width; row++)
            {
                for (int col = 0; col < Width; col++)
                {
                    if (predicate(row, col))
                    {
                        sb.Append('X');
                    }
                    else
                    {
                        sb.Append(' ');
                    }
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public void PopulateData(byte[] rawData)
        {
            MemoryStream ms = new MemoryStream(rawData);

            foreach (var placement in MappingUtil.GetCodewordPlacements(this))
            {
                var thisByte = (uint)ms.ReadByte();
                if (thisByte > Byte.MaxValue)
                {
                    Debug.Fail("Didn't expect to be here.");
                    return;
                }
                _modules.Set(placement.Bit7.Row, placement.Bit7.Col, GetTypeFromBitPosition(thisByte, 7));
                _modules.Set(placement.Bit6.Row, placement.Bit6.Col, GetTypeFromBitPosition(thisByte, 6));
                _modules.Set(placement.Bit5.Row, placement.Bit5.Col, GetTypeFromBitPosition(thisByte, 5));
                _modules.Set(placement.Bit4.Row, placement.Bit4.Col, GetTypeFromBitPosition(thisByte, 4));
                _modules.Set(placement.Bit3.Row, placement.Bit3.Col, GetTypeFromBitPosition(thisByte, 3));
                _modules.Set(placement.Bit2.Row, placement.Bit2.Col, GetTypeFromBitPosition(thisByte, 2));
                _modules.Set(placement.Bit1.Row, placement.Bit1.Col, GetTypeFromBitPosition(thisByte, 1));
                _modules.Set(placement.Bit0.Row, placement.Bit0.Col, GetTypeFromBitPosition(thisByte, 0));

                if (ms.Position == ms.Length)
                {
                    break;
                }
            }

            var nextByte = ms.ReadByte();
            Debug.Assert(nextByte == -1);
        }
    }
}
