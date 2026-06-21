using System.Collections;
using UnityEngine;

public class MobilePerformanceController : MonoBehaviour
{
    private const string PerformanceTierPrefKey = "MobilePerformanceController.PerformanceTier";
    private const string LowTierValue = "low";
    private const string HighTierValue = "high";
    private const float SampleDurationSeconds = 5f;
    private const float LowFpsThreshold = 20f;

    private bool hasApplicationFocus = true;
    private bool isApplicationPaused;
    private bool skipNextSampleFrame;

    void Awake()
    {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
        Debug.unityLogger.filterLogType = LogType.Error | LogType.Exception | LogType.Assert;
#endif
    }

    private void SetLow()
    {
        SetQualityLevel("MobileLow");
        Application.targetFrameRate = 30;
    }

    private void SetHigh()
    {
        SetQualityLevel("MobileHigh");
        Application.targetFrameRate = 60;
    }

    void Start()
    {
        if (TryApplySavedPerformanceTier())
        {
            return;
        }

        StartCoroutine(MeasureAndApplyPerformanceTier());
    }

    private bool TryApplySavedPerformanceTier()
    {
        if (!FilePrefs.HasKey(PerformanceTierPrefKey))
        {
            return false;
        }

        string savedTier = FilePrefs.GetString(PerformanceTierPrefKey, string.Empty);
        if (savedTier == LowTierValue)
        {
            SetLow();
            return true;
        }

        if (savedTier == HighTierValue)
        {
            SetHigh();
            return true;
        }

        return false;
    }

    private IEnumerator MeasureAndApplyPerformanceTier()
    {
        float elapsed = 0f;
        int frameCount = 0;

        while (elapsed < SampleDurationSeconds)
        {
            yield return null;

            if (!CanSampleCurrentFrame())
            {
                continue;
            }

            elapsed += Time.unscaledDeltaTime;
            frameCount++;
        }

        float averageFps = elapsed > 0f ? frameCount / elapsed : 0f;
        if (averageFps < LowFpsThreshold)
        {
            SetLow();
            SavePerformanceTier(LowTierValue);
            yield break;
        }

        SetHigh();
        SavePerformanceTier(HighTierValue);
    }

    private void SavePerformanceTier(string tier)
    {
        FilePrefs.SetString(PerformanceTierPrefKey, tier);
        FilePrefs.Save();
    }

    private void SetQualityLevel(string qualityName)
    {
        int qualityLevel = System.Array.IndexOf(QualitySettings.names, qualityName);
        if (qualityLevel < 0)
        {
            Debug.LogWarning("Quality level not found: " + qualityName);
            return;
        }
        var currentLevel = QualitySettings.GetQualityLevel();
        if (currentLevel == qualityLevel)
        {
            return;
        }

        QualitySettings.SetQualityLevel(qualityLevel, true);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        hasApplicationFocus = hasFocus;

        if (hasFocus)
        {
            skipNextSampleFrame = true;
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        isApplicationPaused = pauseStatus;

        if (!pauseStatus)
        {
            skipNextSampleFrame = true;
        }
    }

    private bool CanSampleCurrentFrame()
    {
        if (!hasApplicationFocus || isApplicationPaused)
        {
            skipNextSampleFrame = true;
            return false;
        }

        if (skipNextSampleFrame)
        {
            skipNextSampleFrame = false;
            return false;
        }

        return true;
    }
}
