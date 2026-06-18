using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;

public class CompleteElementCmd
{
    public void Run(CityElement cityElement)
    {
        var completed = cityElement.dataContainer.ElementCompleted();
        if (!completed)
        {
            cityElement.dataContainer.SetAll(BrickState.Full);
            cityElement.ShowCurrentState();
        }
        var delay = 0.25f;
        new SoundCmd(SoundModel.Instance.CAM_MOVE_BACK, delay).Run();
        DOVirtual.DelayedCall(delay, CamModel.Instance.MoveCamBack);
        DOVirtual.DelayedCall(0.4f + delay, ViewModel.Instance.PlayAnnouncer);
        CityModel.Instance.OnElementCompleted?.Invoke(cityElement);
        ViewModel.Instance.ChangeBottomNav(BottomNav.FinishElement);
    }

}