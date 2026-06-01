using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class SwitchSettingsCmd
{
    public void Run(SettingsKey key)
    {
        var isEnabled = PlayerModel.Instance.IsSettingEnabled(key);
        PlayerModel.Instance.EnableSetting(key, !isEnabled);
    }

}