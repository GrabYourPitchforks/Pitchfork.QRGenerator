using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pitchfork.QRGenerator
{
    internal static class MappingUtil
    {
        internal static IEnumerable<Coord> GetBitStreamMapping(SymbolTemplate symbol)
        {
            // See ISO/IEC 18004:2006(E), Sec. 6.7.3 for information on where codewords
            // are placed in the matrix.

            int width = symbol.Width;

            bool isMovingUpward = true;
            for (int col = width - 1; col > 0; col -= 2)
            {
                // Column six is used as a timing pattern and is skipped for codeword placement
                if (col == 6) { col--; }

                if (isMovingUpward)
                {
                    for (int row = width - 1; row >= 0; row--)
                    {
                        if (!symbol.IsReservedModule(row, col)) { yield return new Coord(row, col); }
                        if (!symbol.IsReservedModule(row, col - 1)) { yield return new Coord(row, col - 1); }
                    }
                }
                else
                {
                    for (int row = 0; row < width; row++)
                    {
                        if (!symbol.IsReservedModule(row, col)) { yield return new Coord(row, col); }
                        if (!symbol.IsReservedModule(row, col - 1)) { yield return new Coord(row, col - 1); }
                    }
                }

                isMovingUpward = !isMovingUpward; // reverse direction when hitting the end of the matrix
            }
        }

        internal static IEnumerable<CodewordPlacement> GetCodewordPlacements(SymbolTemplate symbol)
        {
            var bitstreamMapping = GetBitStreamMapping(symbol);
            return GetCodewordPlacementsImpl(bitstreamMapping);
        }

        private static IEnumerable<CodewordPlacement> GetCodewordPlacementsImpl(IEnumerable<Coord> bitstreamMapping)
        {
            using (var enumerator = bitstreamMapping.GetEnumerator())
            {
                while (true)
                {
                    yield return new CodewordPlacement(
                        bit7: GetNextCoord(enumerator),
                        bit6: GetNextCoord(enumerator),
                        bit5: GetNextCoord(enumerator),
                        bit4: GetNextCoord(enumerator),
                        bit3: GetNextCoord(enumerator),
                        bit2: GetNextCoord(enumerator),
                        bit1: GetNextCoord(enumerator),
                        bit0: GetNextCoord(enumerator));
                }
            }
        }

        private static Coord GetNextCoord(IEnumerator<Coord> coords)
        {
            bool hasNext = coords.MoveNext();
            Debug.Assert(hasNext);
            if (!hasNext)
            {
                throw new InvalidOperationException();
            }
            return coords.Current;
        }
    }
}
