using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;

public class SoundCmd
{
    private string sndName;
    private float delay;

    public SoundCmd(string sndName, float delay = 0)
    {
        this.sndName = sndName;
        this.delay = delay;
    }

    public void Run()
    {
        var isMusic = SoundModel.Instance.MUSIC1 == sndName;

        if (isMusic)
        {
            var musicEnabled = PlayerModel.Instance.IsSettingEnabled(SettingsKey.Music);
            if (!musicEnabled)
            {
                return;
            }
            var remoteConfig = RemoteConfigModel.Instance.RemoteConfig;
            var musicTrack = remoteConfig.MusicTrack;
            if (musicTrack == 0)
            {
                Debug.Log("SoundCmd: Music track 0, not playing music");
                return;
            }
        }

        var isSfx = !isMusic;
        if (isSfx)
        {
            var vibrationsEnabled = PlayerModel.Instance.IsSettingEnabled(SettingsKey.Vibrations);
            if (vibrationsEnabled)
            {
                if (sndName == SoundModel.Instance.BRICK_CLICK)
                {
                    iOSHapticFeedback.Instance.Trigger(iOSHapticFeedback.iOSFeedbackType.ImpactLight);
                }
                else
                {
                    iOSHapticFeedback.Instance.Trigger(iOSHapticFeedback.iOSFeedbackType.ImpactMedium);
                }
            }
            var sfxEnabled = PlayerModel.Instance.IsSettingEnabled(SettingsKey.Sounds);
            if (!sfxEnabled)
            {
                Debug.Log("SoundCmd: SFX disabled, not playing sound: " + sndName);
                return;
            }
        }

        if (isMusic)
        {

        }

        if (delay > 0)
        {
            DOVirtual.DelayedCall(delay, () =>
            {
                SoundModel.Instance.Play(sndName);
            }, false);
        }
        else
        {
            SoundModel.Instance.Play(sndName);
        }
    }

}