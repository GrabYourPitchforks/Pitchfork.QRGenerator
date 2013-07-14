using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pitchfork.QRGenerator
{
    internal static class ArraySegmentExtensions
    {
        public static void CopyToArray<T>(this ArraySegment<T> input, T[] destination)
        {
            Buffer.BlockCopy(input.Array, input.Offset, destination, 0, input.Count);
        }
    }
}
