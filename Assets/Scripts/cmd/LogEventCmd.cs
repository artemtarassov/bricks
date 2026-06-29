using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class LogEventCmd
{
    public static void OutOfSpace()
    {
        {
            var currentElement = ModelUtils.GetCurrentElement();
            var currentGroupIndex = ModelUtils.GetCurrentGroupIndex();
            var dict = new Dictionary<string, object>();
            dict["difficultyIndex"] = PlayerModel.Instance.playerData.difficultyIndex;
            dict["elementName"] = currentElement.dataKey;
            dict["themeIndex"] = currentGroupIndex;
            dict["attempts"] = PlayerModel.Instance.playerData.currentBuildingAttempts;
            dict["coins"] = PlayerModel.Instance.playerData.coins;
            new LogEventCmd().Run("out_of_space", dict);
        }
    }

    public static void OutOfTime()
    {
        {
            var dict = new Dictionary<string, object>();
            dict["buildingName"] = PlayerModel.Instance.playerData.GetCurrentBuildingProgress().BuildingName.ToString();
            new LogEventCmd().Run("out_of_time", dict);
        }
    }

    private static UnityAnalyticsCache cache = null;
    public void Run(string logEventName, Dictionary<string, object> dict)
    {
        if (cache == null)
        {
            cache = new UnityAnalyticsCache();
        }
        dict["secondsPlaying"] = PlayerModel.Instance.playerData.secondsPlaying;
        cache.Send(logEventName, dict);
    }

    public void Run(string logEventName, string key, int value)
    {
        var dict = new Dictionary<string, object>();
        dict[key] = value;
        Run(logEventName, dict);
    }

    public void Run(string logEventName, string key, string value)
    {
        var dict = new Dictionary<string, object>();
        dict[key] = value;
        Run(logEventName, dict);
    }

}