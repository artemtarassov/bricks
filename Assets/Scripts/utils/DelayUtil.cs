using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;
using DG.Tweening;
using System;

public class DelayUtil
{


    public static Sequence Create(object target, float delay, TweenCallback callback)
    {
        Assert.IsNotNull(target, "DelayUtil Create: target is null");
        Assert.IsNotNull(callback, "DelayUtil Create: callback is null");

        return DOTween.Sequence(target)
            .AppendInterval(delay)
            .AppendCallback(() =>
            {
                try
                {
                    callback?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError("DelayUtil Create: callback exception: " + e.ToString());
                }
            });
    }

    public static void NextFrame(GameObject target, Action callback)
    {
        Assert.IsNotNull(target, "DelayUtil Create: target is null");
        Assert.IsNotNull(callback, "DelayUtil Create: callback is null");
        target.AddComponent<FrameWait>().OnNextFrame = callback;
    }

}

public class FrameWait : MonoBehaviour
{
    public Action OnNextFrame;
    void Update()
    {
        try
        {
            OnNextFrame?.Invoke();
        }
        catch (Exception)
        {
            //ignore
        }
        Destroy(this);
    }
}