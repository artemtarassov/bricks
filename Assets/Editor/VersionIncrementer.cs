using UnityEditor;
using UnityEngine;

public static class VersionIncrementer
{
    [MenuItem("Build/Increment Version And Build Numbers")]
    public static void IncrementVersionAndBuildNumbers()
    {
        var currentVersion = PlayerSettings.bundleVersion;
        var currentIosBuildNumber = PlayerSettings.iOS.buildNumber;
        var currentAndroidBuildNumber = PlayerSettings.Android.bundleVersionCode;

        if (!TryIncrementLastNumericSegment(currentVersion, out var nextVersion))
        {
            Debug.LogError("Could not increment bundle version. Expected the last version segment to be numeric.");
            return;
        }

        if (!TryIncrementLastNumericSegment(currentIosBuildNumber, out var nextIosBuildNumber))
        {
            Debug.LogError("Could not increment iOS build number. Expected the last version segment to be numeric.");
            return;
        }

        var nextAndroidBuildNumber = currentAndroidBuildNumber + 1;

        PlayerSettings.bundleVersion = nextVersion;
        PlayerSettings.iOS.buildNumber = nextIosBuildNumber;
        PlayerSettings.Android.bundleVersionCode = nextAndroidBuildNumber;

        AssetDatabase.SaveAssets();

        Debug.Log(
            "Incremented app version and build numbers. " +
            "Version: " + currentVersion + " -> " + nextVersion +
            ", iOS build: " + currentIosBuildNumber + " -> " + nextIosBuildNumber +
            ", Android build: " + currentAndroidBuildNumber + " -> " + nextAndroidBuildNumber);
    }

    private static bool TryIncrementLastNumericSegment(string value, out string incrementedValue)
    {
        incrementedValue = value;

        if (string.IsNullOrWhiteSpace(value))
        {
            incrementedValue = "1";
            return true;
        }

        var segments = value.Split('.');
        var lastSegmentIndex = segments.Length - 1;
        var lastSegment = segments[lastSegmentIndex];

        if (!int.TryParse(lastSegment, out var lastNumber))
        {
            return false;
        }

        segments[lastSegmentIndex] = (lastNumber + 1).ToString(new string('0', lastSegment.Length));
        incrementedValue = string.Join(".", segments);
        return true;
    }
}
