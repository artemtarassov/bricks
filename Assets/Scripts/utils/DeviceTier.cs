using UnityEngine;

public static class DeviceTier
{
    public static bool IsLowEnd()
    {
        var maxScreenDimension = Mathf.Max(Screen.width, Screen.height);
        var hasReportedGpuMemory = SystemInfo.graphicsMemorySize > 0;

        return SystemInfo.graphicsShaderLevel < 35
               || SystemInfo.systemMemorySize <= 3000
               || SystemInfo.processorCount <= 6
               || (hasReportedGpuMemory && SystemInfo.graphicsMemorySize <= 1024)
               || (maxScreenDimension >= 2400 && hasReportedGpuMemory && SystemInfo.graphicsMemorySize <= 1536);
    }
}
