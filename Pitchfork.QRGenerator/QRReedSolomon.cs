using System;
using System.Diagnostics;
using System.Threading;

namespace Pitchfork.QRGenerator
{
    internal static class QRReedSolomon
    {
        private static readonly GaloisField _qrGaloisField = new GaloisField(0x11d); // 0x11d is the polynomial for the QR Code GF(2^8)
        private static readonly byte[][] _generatorPolynomials = CreateDefaultGeneratorPolynomials();

        private static byte[][] CreateDefaultGeneratorPolynomials()
        {
            byte[][] generatorPolynomials = new byte[128][];
            generatorPolynomials[1] = new byte[] { 1, 1 }; // g_1(x) = x + 1, which is the first-degree polynomial with root 2^0 in GF(2^8)
            return generatorPolynomials;
        }

        private static byte[] GetGeneratorPolynomial(int degree)
        {
            // In a Reed-Solomon code, the generator polynomial of degree n is a polynomial
            // with roots 2^0, 2^1, ..., 2^(n-1). This is also expressed as:
            // g_n(x) = (x - 2^0) * (x - 2^1) * ... * (x - 2^(n-1)).

            Debug.Assert(1 <= degree && degree < _generatorPolynomials.Length);
            byte[] generatorPolynomial = Volatile.Read(ref _generatorPolynomials[degree]);
            if (generatorPolynomial == null)
            {
                byte[] previousGeneratorPolynomial = GetGeneratorPolynomial(degree - 1); // g_{n-1}, which is a polynomial of degree n-1
                byte[] multiplicand = new byte[] { 1, _qrGaloisField.Pow(2, degree - 1) }; // x + 2^(n-1), which is the first-degree polynomial with root 2^(n-1) in GF(2^8)
                generatorPolynomial = _qrGaloisField.MulPoly(previousGeneratorPolynomial, multiplicand); // g_n(x) = g_{n-1}(x) * (x + 2^(n-1)), which results in an n-degree polynomial
                Volatile.Write(ref _generatorPolynomials[degree], generatorPolynomial); // might overwrite existing value, but individual elements are immutable, so whatever
            }
            return generatorPolynomial;
        }

        public static byte[] GetErrorCorrectionBytes(ArraySegment<byte> input, int numErrorCorrectionBytes)
        {
            // In Reed-Solomon, the error correction bytes are the remainder once the input
            // (which has been multiplied by x^n) is divided by g_n(x) over GF(2^8).
            byte[] shiftedInput = new byte[input.Count + numErrorCorrectionBytes];
            input.CopyToArray(shiftedInput);
            byte[] generatorPolynomial = GetGeneratorPolynomial(numErrorCorrectionBytes);
            return _qrGaloisField.PolyMod(shiftedInput, generatorPolynomial);
        }

        public static byte[] GetErrorCorrectionBytes(byte[] input, int numErrorCorrectionBytes)
        {
            return GetErrorCorrectionBytes(new ArraySegment<byte>(input), numErrorCorrectionBytes);
        }
    }
}
