using Unity.VisualScripting;
using UnityEngine.Assertions;

public class BtnCmd
{

    public enum BtnAction
    {
        Restart,
        RefillAttempts,
        FreeAttemptForAd,
        AddSpaceForAd,
        AddSpaceForIAP,
        Continue,
        UseAttemptAndContinue
    }

    private PlayerModel playerModel => PlayerModel.Instance;
    private CityModel cityModel => CityModel.Instance;
    private RemoteConfigData remoteConfig => RemoteConfigModel.Instance.RemoteConfig;

    private BuildingName buildingName;
    private BuildingProgressData progress => playerModel.playerData.GetBuildingProgressByName(buildingName);
    private int maxAttempts = RemoteConfigModel.Instance.RemoteConfig.MaxAttempts;

    public BtnCmd(BuildingName buildingName)
    {
        this.buildingName = buildingName;
    }

    public void Run(BtnAction action)
    {
        var isChallengeBuilding = BuildingNameUtil.IsChallengeBuilding(buildingName);

        if (action == BtnAction.Restart)
        {
            var FillAttemptsAfterRestart = this.remoteConfig.FillAttemptsAfterRestart;
            if (FillAttemptsAfterRestart && !isChallengeBuilding)
            {
                //when player lost and restarts the building from the beginning he gets all hits attempts back
                playerModel.FillAttempts(buildingName, maxAttempts, maxAttempts);
            }
            playerModel.SetCurrentBuilding(buildingName, BuildingState.Playing);
            cityModel.SetCurrentBuildingName(buildingName);
            new CurrentBuildingOperationCmd(CurrentBuildingOperationCmd.NextOperation.RestartBuilding).Run();
            return;
        }
        if (action == BtnAction.RefillAttempts)
        {
            var full = progress.attempts >= maxAttempts;
            if (full)
            {
                Toast("Attempts are already full");
                return;
            }
            var costs = remoteConfig.RefillCoins;
            if (!playerModel.CanAfford(costs))
            {
                Toast("Not enough coins");
                return;
            }
            new AddCoinsCmd(-costs).Run();
            playerModel.FillAttempts(this.buildingName, maxAttempts, maxAttempts);
            return;
        }

        if (action == BtnAction.Continue)
        {
            playerModel.SetCurrentBuilding(buildingName, BuildingState.Playing);
            cityModel.SetCurrentBuildingName(buildingName);

            var curElement = progress.GetCurrentElement();
            if (curElement == null || isChallengeBuilding)
            {
                new CurrentBuildingOperationCmd(CurrentBuildingOperationCmd.NextOperation.RestartBuilding).Run();
            }
            else
            {
                if (curElement.ElementCompleted())
                {
                    new CurrentBuildingOperationCmd(CurrentBuildingOperationCmd.NextOperation.NextElement).Run();
                }
                else
                {
                    new CurrentBuildingOperationCmd(CurrentBuildingOperationCmd.NextOperation.ContinueCurrentElement).Run();
                }
            }
            return;
        }
        if (action == BtnAction.UseAttemptAndContinue)
        {
            var didUse = playerModel.UseAttempt(this.buildingName);
            if (!didUse)
            {
                Toast("No attempts left");
                return;
            }
            playerModel.SetCurrentBuilding(buildingName, BuildingState.Playing);
            cityModel.SetCurrentBuildingName(buildingName);
            new CurrentBuildingOperationCmd(CurrentBuildingOperationCmd.NextOperation.RestartElement).Run();
            return;
        }

        if (action == BtnAction.FreeAttemptForAd)
        {
            new ShowAdCmd().Run(RewardName.ADD_ATTEMPT, this.buildingName);
            return;
        }
    }



    private void Toast(string message)
    {
        new ToastCmd(message).Run();
    }
}