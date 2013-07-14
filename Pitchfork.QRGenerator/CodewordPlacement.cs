using System;

namespace Pitchfork.QRGenerator
{
    internal struct CodewordPlacement
    {
        public readonly Coord Bit7;
        public readonly Coord Bit6;
        public readonly Coord Bit5;
        public readonly Coord Bit4;
        public readonly Coord Bit3;
        public readonly Coord Bit2;
        public readonly Coord Bit1;
        public readonly Coord Bit0;

        public CodewordPlacement(Coord bit7, Coord bit6, Coord bit5, Coord bit4, Coord bit3, Coord bit2, Coord bit1, Coord bit0)
        {
            Bit7 = bit7;
            Bit6 = bit6;
            Bit5 = bit5;
            Bit4 = bit4;
            Bit3 = bit3;
            Bit2 = bit2;
            Bit1 = bit1;
            Bit0 = bit0;
        }
    }
}
