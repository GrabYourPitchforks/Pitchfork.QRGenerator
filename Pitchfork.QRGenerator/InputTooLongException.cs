using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pitchfork.QRGenerator
{
    [Serializable]
    public class InputTooLongException : Exception
    {
        public InputTooLongException() { }
        public InputTooLongException(string message) : base(message) { }
        public InputTooLongException(string message, Exception inner) : base(message, inner) { }
        protected InputTooLongException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
