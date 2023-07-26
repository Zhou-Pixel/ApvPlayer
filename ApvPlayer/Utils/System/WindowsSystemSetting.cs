using System.Runtime.InteropServices;

namespace ApvPlayer.Utils.System;

public class WindowsSystemSetting : ISystemSetting
{
    [DllImport("kernel32")]
    private static extern ExecutionState SetThreadExecutionState(ExecutionState state);

    public void KeepDisplay(bool on)
    {
        ExecutionState state = on
            ? ExecutionState.Continuous | ExecutionState.SystemRequired | ExecutionState.DisplayRequired
            : ExecutionState.Continuous;

        SetThreadExecutionState(state);

    }
}