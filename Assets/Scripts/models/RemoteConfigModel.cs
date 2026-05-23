using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class RemoteConfigModel
{
    public static RemoteConfigModel Instance;
    private RemoteConfigData _remoteConfigData;

    public bool HasSavedData => RemoteConfigData.HasSavedData();
    public bool Applied = false;

    public Action OnRemoteConfigApplied;

    public RemoteConfigData RemoteConfig
    {
        get
        {
            if (_remoteConfigData == null)
            {
                _remoteConfigData = RemoteConfigData.Load();
            }
            return _remoteConfigData;
        }
    }

    public void SetRemoteConfigApplied()
    {
        if (Applied) return;
        Applied = true;
        this.OnRemoteConfigApplied?.Invoke();
    }

}