using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;

public class FlyRocketCmd
{
    public void Run()
    {
        Debug.Log("FlyRocketCmd Run");
        var currentElement = ModelUtils.GetCurrentElement();
        var toPos = currentElement.GetAveragePosition();
        ViewModel.Instance.FlyRocket(GetFromPos(), toPos);
        CamModel.Instance.AnticipateRocketFly();

        DOVirtual.DelayedCall(Durations.RocketFlyDuration, currentElement.Explode);
        DOVirtual.DelayedCall(Durations.RocketFlyDuration + Durations.ExplosionDuration, OnNext);
    }

    private void OnNext()
    {
        if (ModelUtils.CurrentGroupCompleted())
        {
            new CompleteCurrentGroupCmd().Run();
        }
        else
        {
            new UnlockNextCmd().Run();
        }
    }

    private Vector3 GetFromPos()
    {
        var screenPos = ViewModel.Instance.Emitters[1].position;
        var worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 1));
        return worldPos;
    }

}