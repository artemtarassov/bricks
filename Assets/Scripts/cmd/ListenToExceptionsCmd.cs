using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ListenToExceptionsCmd
{
    private static int errorsLogged = 0;
    public void Run()
    {
        AddLogger();
    }

    private void AddLogger()
    {
        Application.logMessageReceived += (condition, stackTrace, type) =>
        {
            if (errorsLogged > 3)
            {
                return; // prevent spam
            }
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
            {
                var urlEncodedCondition = System.Uri.EscapeDataString(condition);
                var urlEncodedStackTrace = System.Uri.EscapeDataString(type + ":" + stackTrace);

                if (urlEncodedStackTrace.Length > 2000)
                {
                    urlEncodedStackTrace = urlEncodedStackTrace.Substring(0, 2000) + "...";
                }
                if (urlEncodedCondition.Length > 2000)
                {
                    urlEncodedCondition = urlEncodedCondition.Substring(0, 2000) + "...";
                }

                var dict = new Dictionary<string, object>();
                dict["conditionEncoded"] = urlEncodedCondition;
                dict["stackTraceEncoded"] = urlEncodedStackTrace;
                dict["errorsLogged"] = errorsLogged;
                try
                {
                    var progress = PlayerModel.Instance.playerData.GetCurrentBuildingProgress();
                    var themeIndex = BuildingNameUtil.GetAllBuildingNames(true).FindIndex(g => g == progress.BuildingName);
                    var currentElement = ModelUtils.GetCurrentElement();
                    dict = new Dictionary<string, object>();
                    dict["themeIndex"] = themeIndex;
                    dict["elementName"] = currentElement == null ? "null" : currentElement.dataKey;
                    dict["progressState"] = progress.State.ToString();
                }
                catch (System.Exception e)
                {
                    Debug.Log("Exception while preparing exception log: " + e);
                    dict = new Dictionary<string, object>();
                    dict["exceptionWhileLogging"] = e.ToString();
                }
                new LogEventCmd().Run("exception_info", dict);
                errorsLogged++;
            }
        };
    }

}