using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;

public class CompleteElementCmd
{
    public void Run(BuildingName buildingName, CityElement cityElement)
    {
        var completed = cityElement.dataContainer.ElementCompleted();
        if (!completed)
        {
            cityElement.dataContainer.SetAll(BrickState.Full);
            cityElement.ShowCurrentState();
        }

        var isChallengeBuilding = BuildingNameUtil.IsChallengeBuilding(buildingName);
        if(isChallengeBuilding)
        {
            ChallengeModel.Instance.CompleteChallenge(buildingName);
        }

        var delay = 0.25f;
        new SoundCmd(SoundModel.Instance.CAM_MOVE_BACK, delay).Run();
        DOVirtual.DelayedCall(delay, CamModel.Instance.MoveCamBack);
        if (RemoteConfigModel.Instance.RemoteConfig.Announcer)
        {
            DOVirtual.DelayedCall(0.4f + delay, ViewModel.Instance.PlayAnnouncer);
        }
        CityModel.Instance.OnElementCompleted?.Invoke(cityElement);

        var fet = cityElement.dataContainer.finishElementType;
        if (fet == FinishElementType.SlideDown || fet == FinishElementType.Undefined)
        {
            ViewModel.Instance.ChangeTopNav(TopNav.None);
            ViewModel.Instance.ChangeBottomNav(BottomNav.FinishElement);
        }
    }

}