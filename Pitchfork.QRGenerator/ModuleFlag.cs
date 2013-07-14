using System;

namespace Pitchfork.QRGenerator
{
    [Flags]
    internal enum ModuleFlag
    {
        // Light module
        Light = 0,

        // Dark module
        Dark = 1 << 0,

        // Reserved module
        Reserved = 1 << 1,
    }
}
