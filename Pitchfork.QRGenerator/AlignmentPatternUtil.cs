using System;
using System.Collections.Generic;

namespace Pitchfork.QRGenerator
{
    internal static class AlignmentPatternUtil
    {
        private static readonly int[][] _coordMappings = GetLookupTable();

        private static int[][] GetLookupTable()
        {
            // Given by ISO/IEC 18004:2006(E), Annex E
            int[][] coords = new int[40][];

            // Version 1
            coords[0] = new int[] { };

            // Version 2
            coords[1] = new int[] { 6, 18 };

            // Version 3
            coords[2] = new int[] { 6, 22 };

            // Version 4
            coords[3] = new int[] { 6, 26 };

            // Version 5
            coords[4] = new int[] { 6, 30 };

            // Version 6
            coords[5] = new int[] { 6, 34 };

            // Version 7
            coords[6] = new int[] { 6, 22, 38 };

            // Version 8
            coords[7] = new int[] { 6, 24, 42 };

            // Version 9
            coords[8] = new int[] { 6, 26, 46 };

            // Version 10
            coords[9] = new int[] { 6, 28, 50 };

            // Version 11
            coords[10] = new int[] { 6, 30, 54 };

            // Version 12
            coords[11] = new int[] { 6, 32, 58 };

            // Version 13
            coords[12] = new int[] { 6, 34, 62 };

            // Version 14
            coords[13] = new int[] { 6, 26, 46, 66 };

            // Version 15
            coords[14] = new int[] { 6, 26, 48, 70 };

            // Version 16
            coords[15] = new int[] { 6, 26, 50, 74 };

            // Version 17
            coords[16] = new int[] { 6, 30, 54, 78 };

            // Version 18
            coords[17] = new int[] { 6, 30, 56, 82 };

            // Version 19
            coords[18] = new int[] { 6, 30, 58, 86 };

            // Version 20
            coords[19] = new int[] { 6, 34, 62, 90 };

            // Version 21
            coords[20] = new int[] { 6, 28, 50, 72, 94 };

            // Version 22
            coords[21] = new int[] { 6, 26, 50, 74, 98 };

            // Version 23
            coords[22] = new int[] { 6, 30, 54, 78, 102 };

            // Version 24
            coords[23] = new int[] { 6, 28, 54, 80, 106 };

            // Version 25
            coords[24] = new int[] { 6, 32, 58, 84, 110 };

            // Version 26
            coords[25] = new int[] { 6, 30, 58, 86, 114 };

            // Version 27
            coords[26] = new int[] { 6, 34, 62, 90, 118 };

            // Version 28
            coords[27] = new int[] { 6, 26, 50, 74, 98, 122 };

            // Version 29
            coords[28] = new int[] { 6, 30, 54, 78, 102, 126 };

            // Version 30
            coords[29] = new int[] { 6, 26, 52, 78, 104, 130 };

            // Version 31
            coords[30] = new int[] { 6, 30, 56, 82, 108, 134 };

            // Version 32
            coords[31] = new int[] { 6, 34, 60, 86, 112, 138 };

            // Version 33
            coords[32] = new int[] { 6, 30, 58, 86, 114, 142 };

            // Version 34
            coords[33] = new int[] { 6, 34, 62, 90, 118, 146 };

            // Version 35
            coords[34] = new int[] { 6, 30, 54, 78, 102, 126, 150 };

            // Version 36
            coords[35] = new int[] { 6, 24, 50, 76, 102, 128, 154 };

            // Version 37
            coords[36] = new int[] { 6, 28, 54, 80, 106, 132, 158 };

            // Version 38
            coords[37] = new int[] { 6, 32, 58, 84, 110, 136, 162 };

            // Version 39
            coords[38] = new int[] { 6, 26, 54, 82, 110, 138, 166 };

            // Version 40
            coords[39] = new int[] { 6, 30, 58, 86, 114, 142, 170 };

            return coords;
        }

        public static IEnumerable<Coord> GetCoords(int version)
        {
            return GetCoordsImpl(_coordMappings[version - 1]);
        }

        private static IEnumerable<Coord> GetCoordsImpl(int[] coordMapping)
        {
            for (int i = 0; i < coordMapping.Length; i++)
            {
                for (int j = 0; j < coordMapping.Length; j++)
                {
                    // We don't return mappings for the top-left, top-right, and bottom-left alignment patterns
                    if ((i == 0 && j == 0) || (i == coordMapping.Length - 1 && j == 0) || (i == 0 && j == coordMapping.Length - 1))
                    {
                        continue;
                    }

                    // Otherwise return this coord pair
                    yield return new Coord(coordMapping[i], coordMapping[j]);
                }
            }
        }
    }
}
