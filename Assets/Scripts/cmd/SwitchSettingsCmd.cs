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

        if (key == SettingsKey.Music)
        {
            var settingEnabled = PlayerModel.Instance.IsSettingEnabled(key);
            if (settingEnabled)
            {
                new SoundCmd(SoundModel.Instance.MUSIC1).Run();
            }
            else
            {
                SoundModel.Instance.Stop(SoundModel.Instance.MUSIC1);
            }
            new LogEventCmd().Run("music_settings", "musicEnabled", settingEnabled ? 1 : 0);
        }
    }

}