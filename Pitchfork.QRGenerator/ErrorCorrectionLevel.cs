using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pitchfork.QRGenerator
{
    /// <summary>
    /// Represents the error correction level present in a QR code image.
    /// </summary>
    public enum ErrorCorrectionLevel
    {
        /// <summary>
        /// The QR code should have approximately 7% data redundancy.
        /// </summary>
        L,

        /// <summary>
        /// The QR code should have approximately 15% data redundancy.
        /// </summary>
        M,

        /// <summary>
        /// The QR code should have approximately 25% data redundancy.
        /// </summary>
        Q,

        /// <summary>
        /// The QR code should have approximately 30% data redundancy.
        /// </summary>
        H
    }
}
