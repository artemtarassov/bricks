using UnityEngine;

public static class DeviceTier
{
    public static bool IsLowEnd()
    {
        Debug.Log($"DeviceTier: graphicsShaderLevel={SystemInfo.graphicsShaderLevel}, systemMemorySize={SystemInfo.systemMemorySize}, graphicsMemorySize={SystemInfo.graphicsMemorySize}, screenResolution={Screen.currentResolution.width}x{Screen.currentResolution.height}");
        // You can check GPU type, shader level, RAM, even Screen.currentResolution
        return SystemInfo.graphicsShaderLevel < 35
               || SystemInfo.systemMemorySize < 2000
               || Screen.currentResolution.width > 1280 && SystemInfo.graphicsMemorySize < 800;
    }
}