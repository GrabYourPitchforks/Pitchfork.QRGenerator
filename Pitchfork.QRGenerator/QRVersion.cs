using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pitchfork.QRGenerator
{
    internal struct QRVersion
    {
        public readonly ErrorCorrectionInfo L;
        public readonly ErrorCorrectionInfo M;
        public readonly ErrorCorrectionInfo Q;
        public readonly ErrorCorrectionInfo H;

        public QRVersion(ErrorCorrectionInfo l, ErrorCorrectionInfo m, ErrorCorrectionInfo q, ErrorCorrectionInfo h)
        {
            Debug.Assert(l.TotalBytes == m.TotalBytes && l.TotalBytes == q.TotalBytes && l.TotalBytes == h.TotalBytes);

            L = l;
            M = m;
            Q = q;
            H = h;
        }

        public ErrorCorrectionInfo GetCorrectionInfo(ErrorCorrectionLevel level)
        {
            switch (level)
            {
                case ErrorCorrectionLevel.L: return L;
                case ErrorCorrectionLevel.M: return M;
                case ErrorCorrectionLevel.Q: return Q;
                case ErrorCorrectionLevel.H: return H;
                default:
                    Debug.Fail("Unknown value.");
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
