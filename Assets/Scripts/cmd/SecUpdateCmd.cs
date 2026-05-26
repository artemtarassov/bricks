using System.Linq;
using UnityEngine;

public class SecUpdateCmd
{
    public void Run()
    {
        UpdateOutOfSpace();
        UpdateAdditionalEmitter();
    }


    private void UpdateOutOfSpace()
    {
        if (ViewModel.Instance.OutOfSpaceFlag)
        {
            Debug.Log("SecUpdateCmd OutOfSpaceFlag is already set, skipping");
            return;
        }
        var currentElement = CityModel.Instance.GetCurrentElement();
        if (currentElement == null || currentElement.dataContainer.ElementCountColoredBricks() > 0)
        {
            return;
        }

        if (ViewModel.Instance.HasAnyView())
        {
            Debug.Log("SecUpdateCmd A view is already active, skipping");
            return;
        }

        var cntEmitterSpace = SlotModel.Instance.CountEmptyEmitters();

        Debug.Log($"SecUpdateCmd cntEmitterSpace: {cntEmitterSpace}");


        if (cntEmitterSpace > 0)
        {
            return;
        }

        var colorsInEmitters = SlotModel.Instance.Emitters.FindAll(e => e.HasColoredBricks).Select(e => e.brickData.color).ToHashSet();
        var colorsInCityElement = CityModel.Instance.GetCurrentElement().GetBrickColors();

        foreach (var c in colorsInEmitters)
        {
            if (colorsInCityElement.Contains(c))
            {
                Debug.Log($"SecUpdateCmd color {c} is still present in emitters, skipping");
                return;
            }
        }

        ViewModel.Instance.OutOfSpaceFlag = true;
        new ShowViewCmd().Run(ViewName.OutOfSpaceView);
    }

    private void UpdateAdditionalEmitter()
    {
        var playerData = PlayerModel.Instance.playerData;

        if (playerData.additionalEmitterUnlockTimeoutTimestamp <= 0)
        {
            return;
        }

        var curTimestamp = TimeUtils.GetUnixTimestamp();
        var timeoutReached = playerData.additionalEmitterUnlockTimeoutTimestamp <= curTimestamp;
        if (!timeoutReached)
        {
            return;
        }

        PlayerModel.Instance.LockAdditionalEmitter();

        if (SlotModel.Instance.Emitters[SlotModel.AdditionalEmitterIndex].IsEmpty)
        {
            SlotModel.Instance.LockAdditionalEmitter();
        }

    }
}