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
        // Predicates given by ISO/IEC 18004:2006(E), Sec. 6.8.1, Table 10
        private delegate bool DataMaskPredicate(int row, int col);
        private static readonly DataMaskPredicate[] _dataMaskPredicates = new DataMaskPredicate[] {
            (row, col) => ((row + col) % 2 == 0), // 000
            (row, col) => (row % 2 == 0), // 001
            (row, col) => (col % 3 == 0), // 010
            (row, col) => ((row + col) % 3 == 0), // 011
            (row, col) => (((row / 2) + (col / 3)) % 2 == 0), // 100
            (row, col) => { int prod = row * col; return ((prod % 2) + (prod % 3) == 0); }, // 101
            (row, col) => { int prod = row * col; return (((prod % 2) + (prod % 3)) % 2 == 0); }, // 110
            (row, col) => ((((row + col) % 2) + ((row * col) % 3)) % 2 == 0), // 111
        };

        private int CalculatePenaltyScore()
        {
            int adjacentModulePenalty = CalculatePenaltyScoreA1();
            int homogenousBlockPenalty = CalculatePenaltyScoreB();
            int finderPatternPenalty = CalculatePenaltyScoreC();
            int unbalancedProportionPenalty = CalculatePenaltyScoreD();
            return adjacentModulePenalty + homogenousBlockPenalty + finderPatternPenalty + unbalancedProportionPenalty;
        }

        // Adjacent modules in row / column in same color
        // Eval condition: # modules = 5 + i
        // Points: 3 + i
        private int CalculatePenaltyScoreA1()
        {
            int totalScore = 0;
            for (int i = 0; i < Width; i++)
            {
                totalScore += CalculatePenaltyScoreA1ForRow(i); // first by row
                totalScore += CalculatePenaltyScoreA1ForCol(i); // then by col
            }
            return totalScore;
        }

        private int CalculatePenaltyScoreA1ForRow(int row)
        {
            int currentRowScore = 0;

            // start with col 0
            int currentRunLength = 1;
            bool currentRunIsDark = _modules.IsDark(row, 0);

            for (int col = 1; col < Width; col++)
            {
                bool currentModuleIsDark = _modules.IsDark(row, col);
                if (currentModuleIsDark == currentRunIsDark)
                {
                    currentRunLength++;
                }
                else
                {
                    if (currentRunLength >= 5) currentRowScore += currentRunLength - 2;
                    currentRunLength = 1;
                    currentRunIsDark = currentModuleIsDark;
                }
            }

            if (currentRunLength >= 5) currentRowScore += currentRunLength - 2;
            return currentRowScore;
        }

        private int CalculatePenaltyScoreA1ForCol(int col)
        {
            int currentColScore = 0;

            // start with row 0
            int currentRunLength = 1;
            bool currentRunIsDark = _modules.IsDark(0, col);

            for (int row = 1; row < Width; row++)
            {
                bool currentModuleIsDark = _modules.IsDark(row, col);
                if (currentModuleIsDark == currentRunIsDark)
                {
                    currentRunLength++;
                }
                else
                {
                    if (currentRunLength >= 5) currentColScore += currentRunLength - 2;
                    currentRunLength = 1;
                    currentRunIsDark = currentModuleIsDark;
                }
            }

            if (currentRunLength >= 5) currentColScore += currentRunLength - 2;
            return currentColScore;
        }

        // Block of modules in same color
        // Eval condition: block size = m * n
        // Points: 3 * (m - 1) * (n - 1)
        private int CalculatePenaltyScoreB()
        {
            // Let's simplify the calculation by using the fact that the number of 2x2 squares
            // that fit into a block of size m * n is (m - 1) * (n - 1). So we'll just count
            // the number of 2x2 squares and multiply by 3 (the weight per occurrence).

            int numOccurrences = 0;
            for (int row = 0; row < Width - 1; row++)
            {
                for (int col = 0; col < Width - 1; col++)
                {
                    bool currentModuleIsDark = _modules.IsDark(row, col);
                    bool is2x2Square = (currentModuleIsDark == _modules.IsDark(row, col + 1))
                        && (currentModuleIsDark == _modules.IsDark(row + 1, col))
                        && (currentModuleIsDark == _modules.IsDark(row + 1, col + 1));
                    if (is2x2Square) { numOccurrences++; }
                }
            }

            return numOccurrences * 3;
        }

        // Unexpected finder pattern present in symbol
        // Eval condition: 1:1:3:1:1 ratio dark:light:dark:light:dark preceded or followed by light area 4 modules wide
        // Points: 40 points per occurrence
        private int CalculatePenaltyScoreC()
        {
            int numOccurrences = 0;

            for (int row = 0; row < Width; row++)
            {
                FinderPatternDetector detector = new FinderPatternDetector();
                for (int col = 0; col < Width; col++)
                {
                    bool currentModuleIsDark = _modules.IsDark(row, col);
                    detector.ShiftIn(currentModuleIsDark);
                }
                numOccurrences += detector.NumFinderPatternsFound;
            }

            for (int col = 0; col < Width; col++)
            {
                FinderPatternDetector detector = new FinderPatternDetector();
                for (int row = 0; row < Width; row++)
                {
                    bool currentModuleIsDark = _modules.IsDark(row, col);
                    detector.ShiftIn(currentModuleIsDark);
                }
                numOccurrences += detector.NumFinderPatternsFound;
            }

            return numOccurrences * 40;
        }

        private int CalculatePenaltyScoreD()
        {
            int totalLightModuleCount = 0, totalDarkModuleCount = 0;

            for (int row = 0; row < Width; row++)
            {
                for (int col = 0; col < Width; col++)
                {
                    bool isDarkModule = _modules.IsDark(row, col);
                    if (isDarkModule) { totalDarkModuleCount++; } else { totalLightModuleCount++; }
                }
            }

            int highestColorAsPercent = (100 * Math.Max(totalDarkModuleCount, totalLightModuleCount)) / (totalDarkModuleCount + totalLightModuleCount);
            int step = (highestColorAsPercent - 50) / 5;
            return step * 10; // weight = 10
        }

        // !! CAUTION !! Mutable struct
        private struct FinderPatternDetector
        {
            internal int NumFinderPatternsFound;

            private const int PATTERN_A = 0x5D0; // 10111010000
            private const int PATTERN_B = 0x5D; // 00001011101
            private const int MASK = 0x7FF; // 11111111111
            private const int NUM_BITS_REQUIRED_FOR_MASK = 11;

            private int _numBitsShiftedIntoRegister;
            private int _currentRegister;

            internal void ShiftIn(bool isDarkModule)
            {
                _currentRegister = (_currentRegister << 1) | ((isDarkModule) ? 1 : 0);
                if (++_numBitsShiftedIntoRegister >= NUM_BITS_REQUIRED_FOR_MASK)
                {
                    int maskedRegisterValue = _currentRegister & MASK;
                    if (maskedRegisterValue == PATTERN_A || maskedRegisterValue == PATTERN_B) { NumFinderPatternsFound++; }
                }
            }
        }
    }
}
