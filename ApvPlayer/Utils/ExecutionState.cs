using System;

namespace ApvPlayer.Utils;

[Flags]
public enum ExecutionState : uint
{
    AwaymodeRequired = 0x00000040,
    Continuous = 0x80000000,
    DisplayRequired = 0x00000002,
    SystemRequired = 0x00000001,
    UserPresent = 0x00000004
}