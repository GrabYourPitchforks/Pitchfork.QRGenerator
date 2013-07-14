using System;
using System.Drawing;

namespace Pitchfork.QRGenerator
{
    /// <summary>
    /// Helper class to generate QR code images.
    /// </summary>
    public static class QRCodeImageGenerator
    {
        /// <summary>
        /// Given an input string, generates a QR code image.
        /// </summary>
        /// <param name="input">The textual data to turn into a QR code image. The text must contain only
        /// printable US-ASCII characters.</param>
        /// <param name="errorCorrectionLevel">The level of data redundancy to include in the generated
        /// image. The default level is 'M' (approximately 15% redundancy).</param>
        /// <returns>An Image corresponding to the generated QR code.</returns>
        public static Image GetQRCode(string input, ErrorCorrectionLevel errorCorrectionLevel = ErrorCorrectionLevel.M)
        {
            ValidateInputString(input);
            if (errorCorrectionLevel < ErrorCorrectionLevel.L || errorCorrectionLevel > ErrorCorrectionLevel.H)
            {
                throw new ArgumentOutOfRangeException("errorCorrectionLevel");
            }

            return Tester.GetQRCode(input);
        }

        private static void ValidateInputString(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            for (int i = 0; i < input.Length; i++)
            {
                char inputChar = input[i];
                if (!((char)0x20 <= inputChar && inputChar <= (char)0x7E))
                {
                    throw new ArgumentException(Res.QRCodeImageTransform_InputContainsInvalidCharacters, "input");
                }
            }
        }
    }
}
