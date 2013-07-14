using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Pitchfork.QRGenerator
{
    // Represents a finite field GF(2^8) with a generator of 2.
    internal sealed class GaloisField
    {
        private readonly byte[] _discreteLogTable;
        private readonly byte[] _exponentiationTable;

        // Creates GF(2^8) from its reducer polynomial.
        // For example, the reducer polynomial x^8 + x^2 + 1
        // is represented by the input value 0x105 (binary 100000101).
        public GaloisField(uint reducerPolynomial)
        {
            Debug.Assert((reducerPolynomial & ~0xff) == 0x100, "Reducer polynomial must have an x^8 element and no higher elements.");

            // Prepare tables
            _exponentiationTable = new byte[255]; // generators can only create 255 elements in GF(2^8)
            _discreteLogTable = new byte[256];

            // Fill in exponentiation and discrete log tables
            uint exponentiationResult = _exponentiationTable[0] = 1; // 2^0 = 1
            for (int i = 1; i < 255; i++)
            {
                exponentiationResult <<= 1; // multiplication by 2 is a simple bit shift
                if ((exponentiationResult & 0x100) != 0)
                {
                    exponentiationResult ^= reducerPolynomial; // modulus is subtraction, which in a GF is represented by XOR
                }
                Debug.Assert(exponentiationResult != 0, "Exponentiation should never result in 0.");
                Debug.Assert((byte)exponentiationResult == exponentiationResult, "Exponentiation result should fit within a byte.");
                _exponentiationTable[i] = (byte)exponentiationResult;

                Debug.Assert(_discreteLogTable[exponentiationResult] == 0, "Discrete log already existed; 2 is not a generator in GF(2^8) with the provided reducer polynomial.");
                _discreteLogTable[exponentiationResult] = (byte)i;
            }
            Debug.Assert(((exponentiationResult << 1) ^ reducerPolynomial) == 1, "2^255 should be 1 in GF(2^8) with the provided reducer polynomial.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte Add(byte a, byte b)
        {
            return (byte)(a ^ b); // addition within a GF of characteristic 2 is identical to XOR
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte Sub(byte a, byte b)
        {
            return (byte)(a ^ b); // subtraction within a GF of characteristic 2 is identical to XOR
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte Mul(byte a, byte b)
        {
            if (a == 0 || b == 0)
            {
                return 0; // anything multiplied by 0 is 0
            }

            // a * b = 2^(log(a)) * 2^(log(b)) = 2^(log(a) + log(b))
            return _exponentiationTable[Mod255(_discreteLogTable[a] + _discreteLogTable[b])];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte Div(byte a, byte b)
        {
            if (b == 0)
            {
                throw new DivideByZeroException();
            }

            // a / b = 2^(log(a)) / 2^(log(b)) = 2^(log(a) - log(b))
            return _exponentiationTable[Mod255(_discreteLogTable[a] - _discreteLogTable[b])];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte Pow(byte a, int b)
        {
            if (a == 0 && b == 0)
            {
                throw new DivideByZeroException();
            }

            if (a == 0) { return 0; }
            if (b == 0) { return 1; }

            // a^b = (2^(log(a))^b = 2^(log(a) * b)
            return _exponentiationTable[Mod255(_discreteLogTable[a] * (long)b)];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Mod255(int value)
        {
            int result = value % 255;
            return (result >= 0) ? result : (result + 255);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Mod255(long value)
        {
            int result = (int)(value % 255);
            return (result >= 0) ? result : (result + 255);
        }

        public byte[] MulPoly(byte[] a, byte[] b)
        {
            int newLen = checked(a.Length + b.Length - 1);
            byte[] newPoly = new byte[newLen];

            for (int i = 0; i < a.Length; i++)
            {
                byte valA = a[i];
                int powA = a.Length - i - 1;
                for (int j = 0; j < b.Length; j++)
                {
                    byte valB = b[j];
                    int powB = b.Length - j - 1;
                    newPoly[newPoly.Length - (powA + powB) - 1] ^= Mul(valA, valB);
                }
            }

            return newPoly;
        }

        public byte[] PolyMod(byte[] dividend, byte[] divisor)
        {
            Debug.Assert(divisor.Length > 1);
            dividend = (byte[])dividend.Clone(); // mutating in place

            for (int i = 0; i + divisor.Length-1 < dividend.Length  ; i++)
            {
                byte multFactor = dividend[i];
                for (int j = 0; j < divisor.Length; j++)
                {
                    dividend[i + j] ^= Mul(multFactor, divisor[j]);
                }
            }

            byte[] remainder = new byte[divisor.Length - 1];
            Buffer.BlockCopy(dividend, dividend.Length - remainder.Length, remainder, 0, remainder.Length);
            return remainder;
        }
    }
}
