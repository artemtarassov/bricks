using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ApplyRemoteConfigCmd
{
    public void Run()
    {
        if (RemoteConfigModel.Instance.Applied)
        {
            return;
        }
        RemoteConfigModel.Instance.SetRemoteConfigApplied();
    }


}