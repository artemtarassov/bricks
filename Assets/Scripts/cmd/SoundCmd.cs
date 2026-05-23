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
        if (delay > 0)
        {
            DOVirtual.DelayedCall(delay, () =>
            {
                SoundModel.Instance.Play(sndName);
            });
        }
        else
        {
            SoundModel.Instance.Play(sndName);
        }
    }

}