using System;

namespace Pitchfork.QRGenerator
{
    internal struct Coord
    {
        public readonly int Row;
        public readonly int Col;

        public Coord(int row, int col)
        {
            Row = row;
            Col = col;
        }
    }
}
