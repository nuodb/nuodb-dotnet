// PkgCmdID.cs
// MUST match PkgCmdID.h
using System;

namespace NuoDb.VisualStudio.DataTools
{
    static class PkgCmdIDList
    {
        public const uint cmdNewConnection = 0x100;
        public const uint cmdConnectionOpen = 0x101;
        public const uint cmdConnectionClose = 0x102;
        public const uint cmdExecuteCommand = 0x103;

    };
}